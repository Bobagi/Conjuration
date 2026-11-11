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
	// A bolt of lightning from the sky onto an enemy hit by the Storm Atronach's Chain Lightning, only when
	// that enemy is under the open sky (surface layer, nothing solid above it). Stronger while it rains.
	// Drawn as a jagged line from above the screen down to the target; the hitbox sits on the target.
	public class StormStrike : ModProjectile
	{
		public const int SkyCheckTiles = 30;
		private const int Life = 18;
		private const float Height = 640f;   // how far above the target the bolt starts
		private Vector2[] points;

		public override string Texture => "Conjuration/Content/Projectiles/Blank";

		// Open sky: above the underground layer and no solid tile in the column above the target.
		public static bool OpenSky(NPC npc)
		{
			if (npc.Top.Y >= Main.worldSurface * 16.0) return false;
			int x = (int)(npc.Center.X / 16f);
			int top = (int)(npc.Top.Y / 16f) - 1;
			for (int y = top; y > top - SkyCheckTiles && y > 0; y--)
			{
				if (!WorldGen.InWorld(x, y)) break;
				Tile t = Main.tile[x, y];
				if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType]) return false;
			}
			return true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Life;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;   // hits once
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
		{
			SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.7f, MaxInstances = 2 }, Projectile.Center);
			for (int i = 0; i < 24; i++)
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 0, default, 1.4f);
				d.velocity = Main.rand.NextVector2Circular(5f, 5f);
				d.noGravity = true;
			}
		}

		// Only the first ticks deal damage; the rest is the fading flash.
		public override bool? CanDamage() => Projectile.timeLeft > Life - 4 ? null : false;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Electrified, 180);
		}

		public override void AI()
		{
			if (points == null || Projectile.timeLeft % 4 == 0) points = Bolt(Projectile.Center);
			Lighting.AddLight(Projectile.Center, 0.9f, 0.85f, 1.2f);
			Lighting.AddLight(Projectile.Center - new Vector2(0f, 200f), 0.5f, 0.45f, 0.8f);
		}

		private static Vector2[] Bolt(Vector2 bottom)
		{
			const int segments = 16;
			var pts = new Vector2[segments + 1];
			for (int i = 0; i <= segments; i++)
			{
				float t = i / (float)segments;
				float jitter = (i == 0 || i == segments) ? 0f : Main.rand.NextFloat(-22f, 22f);
				pts[i] = bottom + new Vector2(jitter, -Height * (1f - t));
			}
			return pts;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (points == null) return false;
			float fade = Projectile.timeLeft / (float)Life;
			Texture2D px = TextureAssets.MagicPixel.Value;
			Rectangle src = new Rectangle(0, 0, 1, 1);
			for (int i = 0; i < points.Length - 1; i++)
			{
				Vector2 a = points[i] - Main.screenPosition, b = points[i + 1] - Main.screenPosition;
				Vector2 d = b - a;
				float rot = d.ToRotation();
				// purple glow, then the white hot core
				Main.EntitySpriteDraw(px, a, src, new Color(150, 110, 255, 0) * fade, rot, new Vector2(0f, 0.5f), new Vector2(d.Length(), 8f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(px, a, src, new Color(255, 255, 255, 0) * fade, rot, new Vector2(0f, 0.5f), new Vector2(d.Length(), 3f), SpriteEffects.None, 0);
			}
			return false;
		}
	}
}
