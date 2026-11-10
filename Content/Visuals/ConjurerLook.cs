using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Conjuration.Content.Visuals
{
	// Look of the Conjurer town NPC, modelled on Skyrim's conjuration master who sells the Flame Atronach
	// tome at the College of Winterhold (Phinis Gestor): bald with grey sides, pale, no beard, long slate
	// blue mage tunic over a brown red shirt, dark trousers, pale wrapped boots. Only the player's own
	// clothes and colours are used (the "Coat" clothing style gives the long tunic).
	// Rendered frame by frame into the town NPC sheet by /atronachtest npc.
	public static class ConjurerLook
	{
		public static int Hair = 15;   // see /atronachtest hairsm

		public static Player Create()
		{
			var p = new Player();
			p.Male = true;
			p.skinVariant = PlayerVariantID.MaleCoat;
			p.hair = Hair;
			p.skinColor = new Color(232, 196, 170);
			p.hairColor = new Color(168, 166, 160);
			p.eyeColor = new Color(90, 100, 120);
			p.shirtColor = new Color(70, 74, 112);       // the slate blue tunic
			p.underShirtColor = new Color(118, 54, 42);  // brown red shirt under it
			p.pantsColor = new Color(54, 56, 80);
			p.shoeColor = new Color(196, 182, 150);      // pale wrapped boots
			for (int i = 0; i < p.armor.Length; i++) p.armor[i] = new Item();
			for (int i = 0; i < p.dye.Length; i++) p.dye[i] = new Item();
			p.isFullbright = true;   // sheets are drawn unlit; the game lights the NPC
			p.width = 20;
			p.height = 42;
			return p;
		}
	}
}
