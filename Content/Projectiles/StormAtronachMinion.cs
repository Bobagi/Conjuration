using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;

namespace Conjuration.Content.Projectiles
{
	// Storm Atronach: floating stone and lightning (it hovers, like in Skyrim). Casts Chain Lightning: a
	// bolt that jumps to a second enemy after the first hit.
	public class StormAtronachMinion : AtronachMinion
	{
		protected override int BuffType => ModContent.BuffType<StormAtronachBuff>();
		protected override int HitboxWidth => 30;
		protected override int HitboxHeight => 64;
		protected override float HoverHeight => 10f;
		protected override float AttackRange => 560f;
		protected override int AttackCooldown => 54;
		protected override int WindupTicks => 18;
		protected override Vector3 LightColor => new Vector3(0.55f, 0.45f, 1f);
		protected override int AmbientDust => DustID.Electric;

		protected override void Attack(NPC target)
		{
			SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);   // electric zap
			if (Main.myPlayer != Projectile.owner) return;
			Vector2 hand = Projectile.Center + new Vector2(Projectile.direction * 20f, -18f);
			Vector2 vel = (target.Center - hand).SafeNormalize(Vector2.UnitX * Projectile.direction) * StormBolt.Speed;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), hand, vel, ModContent.ProjectileType<StormBolt>(),
				Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 1f);   // ai0 = jumps left
		}
	}
}
