using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Conjuration.Content.Projectiles
{
	// Shared ground AI and drawing for every atronach (Skyrim's atronachs stand on the ground or hover just
	// above it, they do not fly around like the Imp). Each subclass sets its size, speed and attack.
	//
	// Sprite sheet: frames stacked vertically, facing RIGHT, feet on the bottom row of each frame:
	//   0 idle | 1 walk A | 2 pass | 3 walk B | 4 pass | 5 attack
	// Built from the Skyrim references by tools/atronach_art.py (see docs/ART.md).
	public abstract class AtronachMinion : ModProjectile
	{
		public const int FrameCount = 6;
		protected const int FrameIdle = 0, FrameWalk = 1, FrameAttack = 5;

		protected abstract int BuffType { get; }
		protected abstract int HitboxWidth { get; }
		protected abstract int HitboxHeight { get; }
		protected virtual float WalkSpeed => 3.6f;
		protected virtual float RunSpeed => 8f;
		protected virtual float AttackRange => 520f;     // stops and attacks inside this distance (centre to centre)
		protected virtual int AttackCooldown => 68;
		protected virtual int WindupTicks => 14;         // attack pose before the hit or the spell leaves
		protected virtual int RecoverTicks => 12;        // attack pose after it
		protected virtual float HoverHeight => 0f;       // drawn this many pixels above the ground (storm)
		protected virtual float DetectRange => 700f;
		protected virtual Vector3 LightColor => new Vector3(1f, 0.5f, 0.12f);
		protected virtual int AmbientDust => DustID.Torch;
		protected virtual float JumpSpeed => 7.5f;

		// Called once per attack when the wind up ends (owner's client for spawning projectiles).
		protected abstract void Attack(NPC target);

		private const float Gravity = 0.4f, MaxFall = 10f, TeleportDistance = 1000f;

		// ai[0]: attack timer, ai[1]: stuck timer, localAI[0]: cooldown.
		protected ref float AttackTimer => ref Projectile.ai[0];
		private ref float StuckTimer => ref Projectile.ai[1];
		private ref float Cooldown => ref Projectile.localAI[0];
		private bool onGround;
		protected NPC CurrentTarget;

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
			Projectile.width = HitboxWidth;
			Projectile.height = HitboxHeight;
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
		public override bool MinionContactDamage() => false;   // damage comes from Attack()

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = Main.player[Projectile.owner].Bottom.Y > Projectile.Bottom.Y + 24f;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.Y == 0f && oldVelocity.Y > 0f) onGround = true;
			if (Projectile.velocity.X != oldVelocity.X && onGround && Math.Abs(oldVelocity.X) > 0.5f)
			{
				Projectile.velocity.X = oldVelocity.X;
				Projectile.velocity.Y = -JumpSpeed;
				onGround = false;
			}
			return false;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(BuffType);
				return;
			}
			if (owner.HasBuff(BuffType)) Projectile.timeLeft = 2;

			bool wasOnGround = onGround;
			onGround = false;
			NPC target = FindTarget(owner);
			CurrentTarget = target;
			Vector2 toOwner = owner.Bottom - Projectile.Bottom;

			if (toOwner.Length() > TeleportDistance || StuckTimer > 120f)
			{
				TeleportTo(owner);
				return;
			}

			float wantX = 0f;
			if (AttackTimer <= 0f)
			{
				if (target != null && Vector2.Distance(target.Center, Projectile.Center) < AttackRange)
				{
					Face(target.Center.X);
					if (Cooldown <= 0f)
					{
						AttackTimer = WindupTicks + RecoverTicks;
						Cooldown = AttackCooldown;
						Projectile.netUpdate = true;
					}
				}
				else if (target != null)
				{
					wantX = Math.Sign(target.Center.X - Projectile.Center.X) * WalkSpeed;
				}
				else
				{
					float idleX = owner.Center.X - owner.direction * (40f + Projectile.minionPos * 36f);
					float dx = idleX - Projectile.Center.X;
					if (Math.Abs(dx) > 20f)
						wantX = Math.Sign(dx) * (Math.Abs(dx) > 240f ? RunSpeed : WalkSpeed);
					else if (Math.Abs(Projectile.velocity.X) < 0.5f)
						Projectile.direction = Projectile.spriteDirection = owner.direction;
					if (wasOnGround && toOwner.Y < -120f && Math.Abs(dx) < 160f)
						Projectile.velocity.Y = -9f;
				}
			}
			else
			{
				AttackTimer--;
				if (AttackTimer == RecoverTicks && target != null) Attack(target);
			}

			Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, wantX, wantX == 0f ? 0.25f : 0.12f);
			if (Math.Abs(Projectile.velocity.X) < 0.05f) Projectile.velocity.X = 0f;
			if (wantX != 0f) Face(Projectile.Center.X + wantX);

			Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, MaxFall);
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height,
				ref Projectile.stepSpeed, ref Projectile.gfxOffY);

			if (Math.Abs(toOwner.X) > 300f && Math.Abs(Projectile.velocity.X) < 0.5f && target == null) StuckTimer++;
			else StuckTimer = Math.Max(0f, StuckTimer - 2f);

			Cooldown = Math.Max(0f, Cooldown - 1f);
			Animate(wasOnGround);
			Lighting.AddLight(Projectile.Center, LightColor * (0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.3f)));
			if (Main.rand.NextBool(4))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, AmbientDust, 0f, -1f, 100, default, 1.1f);
				d.noGravity = true;
			}
		}

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

		protected void Face(float x) => Projectile.spriteDirection = Projectile.direction = x > Projectile.Center.X ? 1 : -1;

		private void TeleportTo(Player owner)
		{
			Burst();
			Projectile.Bottom = owner.Bottom + new Vector2(-owner.direction * 40f, 0f);
			Projectile.velocity = Vector2.Zero;
			StuckTimer = 0f;
			AttackTimer = 0f;
			Projectile.netUpdate = true;
			Burst();
			SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
		}

		protected void Burst()
		{
			for (int i = 0; i < 24; i++)
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, AmbientDust, 0f, 0f, 100, default, 2f);
				d.velocity *= 2.5f;
				d.noGravity = true;
			}
		}

		private void Animate(bool grounded)
		{
			if (AttackTimer > 0f) { Projectile.frame = FrameAttack; return; }
			if (!grounded && Math.Abs(Projectile.velocity.Y) > 1f && HoverHeight <= 0f) { Projectile.frame = FrameWalk; return; }
			float speed = Math.Abs(Projectile.velocity.X);
			if (speed < 0.4f)
			{
				Projectile.frame = FrameIdle;
				Projectile.frameCounter = 0;
				return;
			}
			Projectile.frameCounter += (int)Math.Ceiling(speed);
			if (Projectile.frame < FrameWalk || Projectile.frame > FrameWalk + 3) Projectile.frame = FrameWalk;
			if (Projectile.frameCounter >= 28)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = Projectile.frame >= FrameWalk + 3 ? FrameWalk : Projectile.frame + 1;
			}
		}

		// Frame bottom centre on the hitbox bottom centre; the sheet faces right.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int fh = tex.Height / FrameCount;
			Rectangle src = new Rectangle(0, Projectile.frame * fh, tex.Width, fh);
			float bob = HoverHeight > 0f ? (float)Math.Sin(Main.GameUpdateCount * 0.08f + Projectile.whoAmI) * 3f : 0f;
			Vector2 pos = Projectile.Bottom - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY + 2f - HoverHeight + bob);
			SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.EntitySpriteDraw(tex, pos, src, Color.White, 0f, new Vector2(tex.Width / 2f, fh), 1f, fx, 0);
			return false;
		}
	}
}
