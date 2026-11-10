using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;
using Conjuration.Content.Visuals;

namespace Conjuration.Content.Projectiles
{
	// Permanent summon minion (1 slot). Unlike the vanilla Imp it does NOT fly: it walks on the
	// ground beside the owner, jumps over steps and walls, and reappears next to the owner in a
	// burst of fire when it falls too far behind. Its attack is the Imp's: it stops, raises its arm
	// and throws a vanilla Imp Fireball (speed 11, pierces, sets On Fire!) at about the Imp's rate.
	//
	// It is drawn with the game's own player renderer (see Visuals/AtronachLook), so the texture file
	// is only a transparent stand in.
	public class FireAtronachMinion : ModProjectile
	{
		private const float DetectRange = 700f;     // from the owner
		private const float ShootRange = 520f;      // stops and throws inside this distance
		private const int AttackCooldown = 68;      // ticks, about the Imp's average rate
		private const int WindupTicks = 14;         // arm raised before the fireball leaves
		private const int ThrowTicks = 12;          // arm stretched toward the target after it leaves
		private const float FireballSpeed = 11f;    // same as the Imp Fireball
		private const float WalkSpeed = 3.6f, RunSpeed = 8f;
		private const float Gravity = 0.4f, MaxFall = 10f;
		private const float TeleportDistance = 1000f;

		// ai[0]: attack timer (counts down from WindupTicks + ThrowTicks while attacking).
		// ai[1]: ticks spent stuck far from the owner. localAI[0]: cooldown between throws.
		private ref float AttackTimer => ref Projectile.ai[0];
		private ref float StuckTimer => ref Projectile.ai[1];
		private ref float Cooldown => ref Projectile.localAI[0];
		private bool onGround;
		private Vector2 aimAt;
		private Player look;   // the dressed up dummy that is drawn, client side only

		public override void SetStaticDefaults()
		{
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;      // same box as a player, so the dummy stands exactly on it
			Projectile.height = 42;
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
			if (target != null) aimAt = target.Center;
			Vector2 toOwner = owner.Bottom - Projectile.Bottom;

			if (toOwner.Length() > TeleportDistance || StuckTimer > 120f)
			{
				TeleportTo(owner);
				return;
			}

			float wantX = 0f;
			if (target != null && Vector2.Distance(target.Center, Projectile.Center) < ShootRange && AttackTimer <= 0f)
			{
				Face(target.Center.X);
				if (Cooldown <= 0f) StartAttack();
			}
			else if (target != null && AttackTimer <= 0f)
			{
				wantX = Math.Sign(target.Center.X - Projectile.Center.X) * WalkSpeed;
			}
			else if (AttackTimer <= 0f)
			{
				float idleX = owner.Center.X - owner.direction * (36f + Projectile.minionPos * 30f);
				float dx = idleX - Projectile.Center.X;
				if (Math.Abs(dx) > 20f)
					wantX = Math.Sign(dx) * (Math.Abs(dx) > 240f ? RunSpeed : WalkSpeed);
				else if (Math.Abs(Projectile.velocity.X) < 0.5f)
					Projectile.direction = Projectile.spriteDirection = owner.direction;   // look where the owner looks
				if (wasOnGround && toOwner.Y < -120f && Math.Abs(dx) < 160f)
					Projectile.velocity.Y = -9f;
			}

			if (AttackTimer > 0f) UpdateAttack(target);

			Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, wantX, wantX == 0f ? 0.25f : 0.12f);
			if (Math.Abs(Projectile.velocity.X) < 0.05f) Projectile.velocity.X = 0f;
			if (wantX != 0f) Face(Projectile.Center.X + wantX);

			Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, MaxFall);
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height,
				ref Projectile.stepSpeed, ref Projectile.gfxOffY);

			if (Math.Abs(toOwner.X) > 300f && Math.Abs(Projectile.velocity.X) < 0.5f && target == null) StuckTimer++;
			else StuckTimer = Math.Max(0f, StuckTimer - 2f);

			Cooldown = Math.Max(0f, Cooldown - 1f);
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

		private Vector2 Hand => Projectile.Center + new Vector2(Projectile.direction * 6f, -8f);

		private void StartAttack()
		{
			AttackTimer = WindupTicks + ThrowTicks;
			Cooldown = AttackCooldown;
			Projectile.netUpdate = true;
		}

		private void UpdateAttack(NPC target)
		{
			AttackTimer--;
			if (AttackTimer > ThrowTicks && Main.rand.NextBool(2))
			{
				// fire gathering in the raised hand
				Dust d = Dust.NewDustDirect(Hand + new Vector2(-Projectile.direction * 6f, -18f) - new Vector2(4f), 8, 8, DustID.Torch, 0f, -1f, 0, default, 1.6f);
				d.noGravity = true;
			}
			if (AttackTimer == ThrowTicks && target != null && Main.myPlayer == Projectile.owner)
			{
				Vector2 vel = (target.Center - Hand).SafeNormalize(Vector2.UnitX * Projectile.direction) * FireballSpeed;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Hand, vel, ProjectileID.ImpFireball,
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

		private void Effects()
		{
			float flicker = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
			Lighting.AddLight(Projectile.Center, 1.1f * flicker, 0.55f * flicker, 0.15f * flicker);
			// The flame hair: embers and fire rising from the head, the body sheds a few sparks.
			if (Main.rand.NextBool(2))
			{
				Vector2 head = Projectile.Top + new Vector2(-Projectile.direction * 2f, 4f);
				Dust d = Dust.NewDustDirect(head - new Vector2(7f, 4f), 14, 8, DustID.Torch, 0f, -2.2f, 0, default, 1.5f);
				d.noGravity = true;
				d.velocity.X = d.velocity.X * 0.3f - Projectile.direction * 0.6f - Projectile.velocity.X * 0.3f;
			}
			if (Main.rand.NextBool(7))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, -1f, 100, default, 1f);
				d.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			look ??= AtronachLook.Create();
			look.direction = Projectile.spriteDirection;
			look.velocity = new Vector2(Projectile.velocity.X, onGround || Math.Abs(Projectile.velocity.Y) < 0.5f ? 0f : Projectile.velocity.Y);
			look.position = Projectile.position + new Vector2(0f, Projectile.gfxOffY);
			look.gfxOffY = 0f;
			AtronachLook.Refresh(look);

			if (AttackTimer > 0f)
			{
				// Raised arm while the fire gathers, then stretched toward the target.
				Vector2 to = aimAt == Vector2.Zero ? new Vector2(Projectile.direction, 0f) : aimAt - Projectile.Center;
				float aim = to.ToRotation() - MathHelper.PiOver2;   // composite arm rotation: 0 = hanging down
				float rot = AttackTimer > ThrowTicks ? (Projectile.direction == 1 ? MathHelper.Pi * 0.85f : -MathHelper.Pi * 0.85f) : aim;
				look.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
				look.legFrame.Y = 0;
				look.bodyFrame.Y = 0;
			}

			Main.PlayerRenderer.DrawPlayer(Main.Camera, look, look.position, 0f, look.fullRotationOrigin);
			return false;
		}
	}
}
