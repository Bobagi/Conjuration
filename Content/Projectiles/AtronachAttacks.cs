using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Conjuration.Content.Projectiles
{
	// The Frost Atronach's punch: an invisible hitbox in front of it for a few ticks. Hits every enemy in
	// it once, with Frostbite.
	public class AtronachSlam : ModProjectile
	{
		public override string Texture => "Conjuration/Content/Projectiles/Blank";

		public override void SetDefaults()
		{
			Projectile.width = 56;
			Projectile.height = 56;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 6;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;   // once per enemy
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = (int)Projectile.ai[0];
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn2, 180);
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}

	// Chain Lightning: a fast bolt drawn with electric dust. On its first hit it jumps once to the nearest
	// other enemy (ai[0] = jumps left) with 75% damage.
	public class StormBolt : ModProjectile
	{
		public const float Speed = 18f;
		public override string Texture => "Conjuration/Content/Projectiles/Blank";

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 60;
			Projectile.extraUpdates = 2;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.Electric, Vector2.Zero, 0, default, 0.9f);
				d.noGravity = true;
			}
			Lighting.AddLight(Projectile.Center, 0.5f, 0.45f, 1f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Electrified, 120);
			if (Projectile.ai[0] < 1f || Main.myPlayer != Projectile.owner) return;
			NPC next = null;
			float best = 320f;
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.whoAmI == target.whoAmI || !npc.CanBeChasedBy(Projectile)) continue;
				float d = Vector2.Distance(npc.Center, target.Center);
				if (d < best) { best = d; next = npc; }
			}
			if (next == null) return;
			Vector2 vel = (next.Center - target.Center).SafeNormalize(Vector2.UnitX) * Speed;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, vel, Type,
				(int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner, ai0: Projectile.ai[0] - 1f);
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}
}
