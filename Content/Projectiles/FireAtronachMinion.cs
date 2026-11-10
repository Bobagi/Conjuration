using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;

namespace Conjuration.Content.Projectiles
{
	// Flame Atronach: walks beside the owner and throws the vanilla Imp Fireball (speed 11, pierces,
	// sets On Fire!) at about the Imp's rate.
	public class FireAtronachMinion : AtronachMinion
	{
		protected override int BuffType => ModContent.BuffType<FireAtronachBuff>();
		protected override int HitboxWidth => 20;
		protected override int HitboxHeight => 44;

		protected override void Attack(NPC target)
		{
			if (Main.myPlayer != Projectile.owner) return;
			Vector2 hand = Projectile.Center + new Vector2(Projectile.direction * 14f, -12f);
			Vector2 vel = (target.Center - hand).SafeNormalize(Vector2.UnitX * Projectile.direction) * 11f;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), hand, vel, ProjectileID.ImpFireball,
				Projectile.damage, Projectile.knockBack, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
		}
	}
}
