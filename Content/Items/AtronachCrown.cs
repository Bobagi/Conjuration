using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Conjuration.Content.Items
{
	// The Flame Atronach's head piece: black mask with glowing eyes, curled horns, a plume of fire,
	// spiked pauldrons and the black chest and hip plates (one player format sheet, drawn by
	// tools/make_atronach_crown.py over the exported body template). The minion's dummy wears it;
	// it also exists as a vanity item so players can dress as an atronach.
	[AutoloadEquip(EquipType.Head)]
	public class AtronachCrown : ModItem
	{
		public override void SetStaticDefaults()
		{
			int slot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
			ArmorIDs.Head.Sets.DrawHead[slot] = false;   // the mask replaces the whole head (no face, no hair)
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.vanity = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 1);
		}
	}
}
