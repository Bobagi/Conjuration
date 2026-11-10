using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Buffs;
using Conjuration.Content.Projectiles;

namespace Conjuration.Content.Items
{
	// Summon item for the Fire Atronach minion. Follows the standard tModLoader minion
	// pattern (see ExampleMod's ExampleSimpleMinionItem): using it adds the buff and spawns
	// the minion at the cursor; the buff keeps the minion alive.
	public class TomeConjureFireAtronach : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 22;               // balancing starts here, see docs/BALANCE.md
			Item.knockBack = 3f;
			Item.mana = 12;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<FireAtronachBuff>();
			Item.shoot = ModContent.ProjectileType<FireAtronachMinion>();
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

		// Temporary recipe so the item can be tested before the wizard NPC exists.
		// The plan is for the NPC to sell it (see README roadmap).
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
}
