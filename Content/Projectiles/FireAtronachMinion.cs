using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;

namespace Conjuration.Content.Projectiles
{
	// Permanent summon minion (1 slot). Unlike the vanilla Imp it does NOT fly: it walks on the
	// ground beside the owner, jumps over steps and walls, and reappears next to the owner in a
	// burst of fire when it falls too far behind. Its attack is the Imp's: it stops, winds up and
	// throws a vanilla Imp Fireball (speed 11, pierces, sets On Fire!) at about the Imp's rate.
	//
	// Sprite sheet (see docs/ART.md): frames of FrameW x FrameH stacked vertically, facing LEFT.
	//   0 idle | 1-4 walk cycle | 5 wind up | 6 throw
	public class FireAtronachMinion : ModProjectile
	{
		public const int FrameW = 52, FrameH = 64;
		private const int FrameCount = 7;
		private const int FrameIdle = 0, FrameWalk = 1, FrameWindup = 5, FrameThrow = 6;

		private const float DetectRange = 700f;     // from the owner
		private const float ShootRange = 520f;      // stops and throws inside this distance
		private const int AttackCooldown = 68;      // ticks, about the Imp's average rate
		private const int WindupTicks = 14;         // wind up pose before the fireball leaves
		private const int ThrowTicks = 12;          // throw pose after it leaves
		private const float FireballSpeed = 11f;    // same as the Imp Fireball
		private const float WalkSpeed = 4.5f, RunSpeed = 9f;
		private const float Gravity = 0.4f, MaxFall = 10f;
		private const float TeleportDistance = 1000f;

		// ai[0]: attack timer (counts down from WindupTicks + ThrowTicks while attacking).
		// ai[1]: ticks spent stuck far from the owner. localAI[0]: cooldown between throws.
		private ref float AttackTimer => ref Projectile.ai[0];
		private ref float StuckTimer => ref Projectile.ai[1];
		private ref float Cooldown => ref Projectile.localAI[0];
		private bool onGround;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = FrameCount;
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 48;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.netImportant = true;
			Projectile.timeLeft = 18000;
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;   // it throws fire, it does not ram

		// Drop through platforms when the owner is below.
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			Player owner = Main.player[Projectile.owner];
			fallThrough = owner.Bottom.Y > Projectile.Bottom.Y + 24f;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.Y == 0f && oldVelocity.Y > 0f) onGround = true;
			// Blocked sideways while standing: hop over it.
			if (Projectile.velocity.X != oldVelocity.X && onGround && Math.Abs(oldVelocity.X) > 0.5f)
			{
				Projectile.velocity.X = oldVelocity.X;
				Projectile.velocity.Y = -7.5f;
				onGround = false;
			}
			return false;   // never die on tiles
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner)) return;

			bool wasOnGround = onGround;
			onGround = false;   // set again by OnTileCollide this tick

			NPC target = FindTarget(owner);
			Vector2 toOwner = owner.Bottom - Projectile.Bottom;

			if (toOwner.Length() > TeleportDistance || StuckTimer > 120f)
			{
				TeleportTo(owner);
				return;
			}

			float wantX = 0f;
			if (target != null && Vector2.Distance(target.Center, Projectile.Center) < ShootRange && AttackTimer <= 0f)
			{
				// In range: stand still and attack.
				Face(target.Center.X);
				if (Cooldown <= 0f) StartAttack();
			}
			else if (target != null && AttackTimer <= 0f)
			{
				// Walk toward the target, but never leave the owner behind.
				float dir = Math.Sign(target.Center.X - Projectile.Center.X);
				wantX = dir * WalkSpeed;
			}
			else if (AttackTimer <= 0f)
			{
				float idleX = owner.Center.X - owner.direction * (36f + Projectile.minionPos * 30f);
				float dx = idleX - Projectile.Center.X;
				if (Math.Abs(dx) > 20f)
					wantX = Math.Sign(dx) * (Math.Abs(dx) > 240f ? RunSpeed : WalkSpeed);
				// Owner well above and close sideways: jump up to them.
				if (wasOnGround && toOwner.Y < -120f && Math.Abs(dx) < 160f)
					Projectile.velocity.Y = -9f;
			}

			if (AttackTimer > 0f) UpdateAttack(target);

			// Horizontal movement with a little acceleration, so it does not snap.
			Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, wantX, wantX == 0f ? 0.25f : 0.12f);
			if (Math.Abs(Projectile.velocity.X) < 0.05f) Projectile.velocity.X = 0f;
			if (wantX != 0f) Face(Projectile.Center.X + wantX);

			Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, MaxFall);
			// Walk up 1 tile steps and slopes smoothly, like the player (no hop needed).
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height,
				ref Projectile.stepSpeed, ref Projectile.gfxOffY);

			// Stuck: far from the owner and not getting closer.
			if (Math.Abs(toOwner.X) > 300f && Math.Abs(Projectile.velocity.X) < 0.5f && target == null) StuckTimer++;
			else StuckTimer = Math.Max(0f, StuckTimer - 2f);

			Cooldown = Math.Max(0f, Cooldown - 1f);
			Animate(wasOnGround);
			Effects();
		}

		private bool CheckActive(Player owner)
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(ModContent.BuffType<FireAtronachBuff>());
				return false;
			}
			if (owner.HasBuff(ModContent.BuffType<FireAtronachBuff>())) Projectile.timeLeft = 2;
			return true;
		}

		// The enemy the owner marked (right click with a summon weapon) first, then the closest visible one.
		private NPC FindTarget(Player owner)
		{
			NPC marked = owner.MinionAttackTargetNPC >= 0 ? Main.npc[owner.MinionAttackTargetNPC] : null;
			if (marked != null && marked.CanBeChasedBy(this) && Vector2.Distance(owner.Center, marked.Center) < DetectRange * 1.5f)
				return marked;

			NPC best = null;
			float bestDist = DetectRange;
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy(this)) continue;
				float d = Vector2.Distance(npc.Center, owner.Center);
				if (d < bestDist && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
				{
					bestDist = d;
					best = npc;
				}
			}
			return best;
		}

		private void Face(float x) => Projectile.spriteDirection = Projectile.direction = x > Projectile.Center.X ? 1 : -1;

		private void StartAttack()
		{
			AttackTimer = WindupTicks + ThrowTicks;
			Cooldown = AttackCooldown;
			Projectile.netUpdate = true;
		}

		private void UpdateAttack(NPC target)
		{
			AttackTimer--;
			// The fireball leaves at the end of the wind up, from the raised hand.
			if (AttackTimer == ThrowTicks && target != null && Main.myPlayer == Projectile.owner)
			{
				Vector2 hand = Projectile.Center + new Vector2(Projectile.direction * 12f, -14f);
				Vector2 vel = (target.Center - hand).SafeNormalize(Vector2.UnitX * Projectile.direction) * FireballSpeed;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), hand, vel, ProjectileID.ImpFireball,
					Projectile.damage, Projectile.knockBack, Projectile.owner);
				SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
			}
		}

		private void TeleportTo(Player owner)
		{
			Burst();
			Projectile.Bottom = owner.Bottom + new Vector2(-owner.direction * 30f, 0f);
			Projectile.velocity = Vector2.Zero;
			StuckTimer = 0f;
			AttackTimer = 0f;
			Projectile.netUpdate = true;
			Burst();
			SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
		}

		private void Burst()
		{
			for (int i = 0; i < 24; i++)
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
				d.velocity *= 2.5f;
				d.noGravity = true;
			}
		}

		private void Animate(bool grounded)
		{
			if (AttackTimer > ThrowTicks) { Projectile.frame = FrameWindup; return; }
			if (AttackTimer > 0f) { Projectile.frame = FrameThrow; return; }
			if (!grounded && Math.Abs(Projectile.velocity.Y) > 1f) { Projectile.frame = FrameWalk; return; }

			float speed = Math.Abs(Projectile.velocity.X);
			if (speed < 0.4f)
			{
				Projectile.frame = FrameIdle;
				Projectile.frameCounter = 0;
				return;
			}
			Projectile.frameCounter += (int)Math.Ceiling(speed);
			if (Projectile.frameCounter >= 24)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = Projectile.frame < FrameWalk || Projectile.frame >= FrameWalk + 3 ? FrameWalk : Projectile.frame + 1;
			}
			else if (Projectile.frame < FrameWalk || Projectile.frame > FrameWalk + 3)
			{
				Projectile.frame = FrameWalk;
			}
		}

		private void Effects()
		{
			float flicker = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
			Lighting.AddLight(Projectile.Center, 1.0f * flicker, 0.5f * flicker, 0.12f * flicker);
			if (Main.rand.NextBool(4))
			{
				// embers rising from the flame hair
				Vector2 head = Projectile.Top + new Vector2(-Projectile.direction * 4f, 2f);
				Dust d = Dust.NewDustDirect(head - new Vector2(6f), 12, 8, DustID.Torch, 0f, -1.5f, 100, default, 1.2f);
				d.noGravity = true;
				d.velocity.X *= 0.4f;
			}
		}

		// Draw the frame with its feet on the bottom of the hitbox (the frame is taller than the hitbox).
		// The sheet faces left, so flip when facing right.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Rectangle src = new Rectangle(0, Projectile.frame * FrameH, FrameW, FrameH);
			Vector2 origin = new Vector2(FrameW / 2f, FrameH);
			Vector2 pos = Projectile.Bottom - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY + 2f);
			SpriteEffects fx = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.EntitySpriteDraw(tex, pos, src, Color.White, 0f, origin, 1f, fx, 0);
			return false;
		}
	}
}
