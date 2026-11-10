using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Items;

namespace Conjuration.Content.Visuals
{
	// The Flame Atronach, modelled on the Skyrim creature (UESP: "female humanoids engulfed in flame,
	// wearing intricate, black, metal armor"): drawn as a dressed up dummy Player with the game's own
	// renderer (the vanilla Mannequin trick), so it walks and gestures like any Terraria character.
	//   - body of fire: the clothes (torso, sleeves, legs) are flame coloured
	//   - black clawed gauntlets: the vanilla Feral Claws with Black Dye (they follow the arm animation)
	//   - black greaves: the shoes
	//   - mask, ram horns, plume of fire, spiked pauldrons, chest and hip plates: the AtronachCrown head piece
	public static class AtronachLook
	{
		public static readonly Color Black = new Color(34, 16, 14);
		public static readonly Color Fire = new Color(255, 160, 48);
		public static readonly Color FireLight = new Color(255, 178, 56);
		public static readonly Color FireLegs = new Color(255, 142, 40);

		public static Player Create(bool withCrown = true)
		{
			var p = new Player();
			p.Male = false;
			p.hair = 15;               // bald: nothing may stick out of the mask
			p.skinColor = Fire;        // the body is fire; the black armor comes from shoes, gauntlets and the crown
			p.eyeColor = FireLight;
			p.shirtColor = Fire;
			p.underShirtColor = FireLight;
			p.pantsColor = FireLegs;
			p.shoeColor = Black;
			for (int i = 0; i < p.armor.Length; i++) p.armor[i] = new Item();
			for (int i = 0; i < p.dye.Length; i++) p.dye[i] = new Item();
			if (withCrown) p.armor[0].SetDefaults(ModContent.ItemType<AtronachCrown>());
			p.armor[3].SetDefaults(ItemID.FeralClaws);   // clawed gauntlets that follow the arm animation...
			p.dye[3].SetDefaults(ItemID.BlackDye);       // ...in black, like the atronach's
			p.skinVariant = PlayerVariantID.FemaleStarter;
			p.isFullbright = true;     // a creature of fire is never in shadow
			p.width = 20;
			p.height = 42;
			return p;
		}

		// Per frame refresh, same order as the vanilla Mannequin before drawing (but NOT flagged as a doll:
		// isDisplayDollOrInanimate swaps the skin for mannequin plastic and ignores skin and hair colours).
		public static void Refresh(Player p)
		{
			p.ResetEffects();
			p.ResetVisibleAccessories();
			p.UpdateDyes();
			p.UpdateSocialShadow();
			p.PlayerFrame();
		}
	}
}
