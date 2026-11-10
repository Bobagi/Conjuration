using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;
using Conjuration.Content.Projectiles;

namespace Conjuration.Content.Items
{
	// Summon tome (standard tModLoader minion item pattern): using it adds the buff and spawns the
	// minion at the cursor; the buff keeps the minion alive. The art is the vanilla Spell Tome with a
	// tint per element (no copied texture). Sold by the Conjurer, see NPCs/Conjurer.
	public abstract class AtronachTome : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.SpellTome;
		protected abstract Color Tint { get; }
		protected abstract int BuffType { get; }
		protected abstract int MinionType { get; }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor.MultiplyRGBA(Tint), 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override Color? GetAlpha(Color lightColor) => lightColor.MultiplyRGBA(Tint);

		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType;
			Item.shoot = MinionType;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;   // the minion appears where you point
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			Projectile p = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, Main.myPlayer);
			p.originalDamage = Item.damage;
			return false;
		}
	}

	// Pre-Hardmode, Hellstone tier (sold after Skeletron; also craftable).
	public class TomeConjureFireAtronach : AtronachTome
	{
		protected override Color Tint => new Color(255, 170, 110);
		protected override int BuffType => ModContent.BuffType<FireAtronachBuff>();
		protected override int MinionType => ModContent.ProjectileType<FireAtronachMinion>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 22;
			Item.knockBack = 3f;
			Item.mana = 12;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Book)
				.AddIngredient(ItemID.HellstoneBar, 12)
				.AddIngredient(ItemID.Fireblossom, 3)
				.AddTile(TileID.Bookcases)
				.Register();
		}
	}

	// Early Hardmode melee tank (sold once Hardmode starts).
	public class TomeConjureFrostAtronach : AtronachTome
	{
		protected override Color Tint => new Color(150, 210, 255);
		protected override int BuffType => ModContent.BuffType<FrostAtronachBuff>();
		protected override int MinionType => ModContent.ProjectileType<FrostAtronachMinion>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 46;
			Item.knockBack = 8f;
			Item.mana = 15;
			Item.value = Item.buyPrice(gold: 8);
			Item.rare = ItemRarityID.LightRed;
		}
	}

	// Mid Hardmode chain lightning (sold after any mechanical boss).
	public class TomeConjureStormAtronach : AtronachTome
	{
		protected override Color Tint => new Color(190, 160, 255);
		protected override int BuffType => ModContent.BuffType<StormAtronachBuff>();
		protected override int MinionType => ModContent.ProjectileType<StormAtronachMinion>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 40;
			Item.knockBack = 2f;
			Item.mana = 18;
			Item.value = Item.buyPrice(gold: 15);
			Item.rare = ItemRarityID.Pink;
		}
	}
}
