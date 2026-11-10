using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;

namespace Conjuration.Content.Projectiles
{
	// Permanent summon minion (takes 1 minion slot, stays until dismissed or the owner dies).
	// Behaviour: hovers beside the owner; when an enemy is in range it keeps its distance and
	// throws fireballs at it. Modeled on ExampleMod's ExampleSimpleMinion, but ranged.
	//
	// ai[0] is unused. localAI[0] is the attack cooldown (ticks).
	public class FireAtronachMinion : ModProjectile
	{
		private const float DetectRange = 700f;     // how far it looks for enemies, from the owner
		private const float KeepDistance = 170f;    // how close it hovers to its target
		private const int ShootCooldown = 45;       // ticks between fireballs
		private const float FireballSpeed = 10f;
		private const float Speed = 9f;             // max travel speed
		private const float Inertia = 16f;          // higher = slower turns
		private const int FrameCount = 4;           // see docs/ART.md

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = FrameCount;
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 38;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			Projectile.timeLeft = 18000;
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;   // it shoots, it does not ram

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner)) return;

			Vector2 idle = IdlePosition(owner);
			float toIdle = Vector2.Distance(Projectile.Center, idle);
			if (toIdle > 2000f)
			{
				// Too far (owner teleported, flew away): snap back.
				Projectile.Center = owner.Center;
				Projectile.netUpdate = true;
			}
			AvoidOverlap();

			FindTarget(owner, out NPC target);

			if (target != null)
			{
				// Hold a spot to the side of the target and above it, then shoot.
				float side = Projectile.Center.X < target.Center.X ? -1f : 1f;
				Vector2 hover = target.Center + new Vector2(side * KeepDistance, -50f);
				MoveToward(hover, 12f);
				Projectile.spriteDirection = Projectile.direction = Projectile.Center.X < target.Center.X ? 1 : -1;
				Shoot(target);
			}
			else
			{
				if (toIdle > 160f) MoveToward(idle, 14f);
				else if (Projectile.velocity == Vector2.Zero)
				{
					Projectile.velocity = new Vector2(-0.15f, -0.05f);   // tiny drift so it never freezes
				}
				else Projectile.velocity *= 0.96f;
				if (Math.Abs(Projectile.velocity.X) > 0.2f)
					Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
			}

			Projectile.localAI[0] = Math.Max(0f, Projectile.localAI[0] - 1f);
			Animate();

			// Warm light so it reads as a fire creature.
			Lighting.AddLight(Projectile.Center, 1.0f, 0.45f, 0.1f);
			if (Main.rand.NextBool(6))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, -1.5f, 100, default, 1.3f);
				d.noGravity = true;
			}
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

		// Several minions line up behind the owner instead of stacking on one point.
		private Vector2 IdlePosition(Player owner)
		{
			Vector2 pos = owner.Center;
			pos.Y -= 48f;
			pos.X += (10f + Projectile.minionPos * 42f) * -owner.direction;
			return pos;
		}

		private void AvoidOverlap()
		{
			const float push = 0.05f;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner
					&& other.type == Projectile.type
					&& Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
				{
					Projectile.velocity.X += Projectile.position.X < other.position.X ? -push : push;
					Projectile.velocity.Y += Projectile.position.Y < other.position.Y ? -push : push;
				}
			}
		}

		// Prefers the enemy the owner right-clicked (summon target), else the closest one
		// the minion can see.
		private void FindTarget(Player owner, out NPC target)
		{
			target = null;
			float best = DetectRange;

			NPC chosen = owner.MinionAttackTargetNPC >= 0 ? Main.npc[owner.MinionAttackTargetNPC] : null;
			if (chosen != null && chosen.CanBeChasedBy(this)
				&& Vector2.Distance(owner.Center, chosen.Center) < DetectRange * 1.6f)
			{
				target = chosen;
				return;
			}

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy(this)) continue;
				float between = Vector2.Distance(npc.Center, owner.Center);
				if (between < best && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
				{
					best = between;
					target = npc;
				}
			}
		}

		private void MoveToward(Vector2 destination, float maxSpeed)
		{
			Vector2 dir = destination - Projectile.Center;
			float dist = dir.Length();
			if (dist < 8f) { Projectile.velocity *= 0.9f; return; }
			dir /= dist;
			dir *= Math.Min(maxSpeed, Math.Max(Speed, dist / 10f));
			Projectile.velocity = (Projectile.velocity * (Inertia - 1f) + dir) / Inertia;
		}

		// Only the owner's client spawns the fireball; it is synced like any projectile.
		private void Shoot(NPC target)
		{
			if (Projectile.localAI[0] > 0f || Main.myPlayer != Projectile.owner) return;
			Projectile.localAI[0] = ShootCooldown;

			Vector2 dir = Vector2.Normalize(target.Center - Projectile.Center);
			int i = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * FireballSpeed,
				ProjectileID.BallofFire, Projectile.damage, Projectile.knockBack, Projectile.owner);
			Main.projectile[i].DamageType = DamageClass.Summon;
			SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
		}

		private void Animate()
		{
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 7)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % FrameCount;
			}
		}

		// Fully bright so the flames do not get darkened by the world lighting.
		public override Color? GetAlpha(Color lightColor) => Color.White;
	}
}
