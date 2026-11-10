using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Conjuration.Content.Visuals
{
	// The Fire Atronach is drawn as a dressed up dummy Player with the game's own renderer (the same
	// trick the vanilla Mannequin uses), so it has Terraria's real proportions, walk cycle and arm poses
	// and no copied art: a woman of living fire with flame coloured hair and Molten armor plates.
	public static class AtronachLook
	{
		public static int Hair = 23;   // the spiky upward style reads as flames; picked from /atronachtest hairs

		public static readonly Color Skin = new Color(255, 128, 40);
		public static readonly Color HairColor = new Color(255, 196, 64);
		public static readonly Color Eyes = new Color(255, 250, 200);
		public static readonly Color Cloth = new Color(255, 96, 24);
		public static readonly Color ClothDark = new Color(200, 56, 16);

		public static Player Create()
		{
			var p = new Player();
			p.Male = false;
			p.hair = Hair;
			p.skinColor = Skin;
			p.hairColor = HairColor;
			p.eyeColor = Eyes;
			p.shirtColor = Cloth;
			p.underShirtColor = Cloth;
			p.pantsColor = ClothDark;
			p.shoeColor = ClothDark;
			for (int i = 0; i < p.armor.Length; i++) p.armor[i] = new Item();
			for (int i = 0; i < p.dye.Length; i++) p.dye[i] = new Item();
			p.armor[1].SetDefaults(ItemID.MoltenBreastplate);   // rock plates over the chest and shoulders
			p.dye[1].SetDefaults(ItemID.FlameAndBlackDye);      // ...dyed black with moving flames
			p.isFullbright = true;                             // a fire creature is never in shadow
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
