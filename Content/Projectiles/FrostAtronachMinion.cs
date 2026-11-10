using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;

namespace Conjuration.Content.Projectiles
{
	// Frost Atronach: Skyrim's only atronach that does not hover, a huge construct of ice that fights in
	// melee. Big and slow; walks up to the enemy and punches (a short hitbox in front of it) for heavy
	// knockback and Frostbite.
	public class FrostAtronachMinion : AtronachMinion
	{
		protected override int BuffType => ModContent.BuffType<FrostAtronachBuff>();
		protected override int HitboxWidth => 36;
		protected override int HitboxHeight => 70;
		protected override float WalkSpeed => 2.6f;
		protected override float RunSpeed => 6.5f;
		protected override float AttackRange => 64f;
		protected override int AttackCooldown => 40;
		protected override int WindupTicks => 16;
		protected override int RecoverTicks => 14;
		protected override float JumpSpeed => 8.5f;
		protected override Vector3 LightColor => new Vector3(0.3f, 0.6f, 0.9f);
		protected override int AmbientDust => DustID.IceTorch;

		protected override void Attack(NPC target)
		{
			SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);   // heavy stone/ice impact
			for (int i = 0; i < 16; i++)
			{
				Dust d = Dust.NewDustDirect(Projectile.Center + new Vector2(Projectile.direction * 30f, -10f), 24, 24, DustID.Ice, Projectile.direction * 3f, -2f, 0, default, 1.4f);
				d.noGravity = i % 2 == 0;
			}
			if (Main.myPlayer != Projectile.owner) return;
			Vector2 fist = Projectile.Center + new Vector2(Projectile.direction * 34f, -6f);
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), fist, Vector2.Zero, ModContent.ProjectileType<AtronachSlam>(),
				Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.direction);
		}
	}
}
