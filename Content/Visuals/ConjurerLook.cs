using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Conjuration.Content.Visuals
{
	// Look of the Conjurer town NPC, used only by the sprite sheet exporter (/atronachtest npc).
	// A dummy Player dressed with vanilla items, rendered frame by frame into the town NPC sheet:
	// that is what the community "Player Renderer" helper does, built into the mod.
	public static class ConjurerLook
	{
		public static Player Create()
		{
			var p = new Player();
			p.Male = true;
			p.hair = 21;
			p.skinColor = new Color(228, 170, 128);
			p.hairColor = new Color(236, 232, 224);   // white haired old mage
			p.eyeColor = new Color(255, 150, 40);      // ember eyes
			p.shirtColor = new Color(70, 30, 30);
			p.underShirtColor = new Color(110, 40, 30);
			p.pantsColor = new Color(60, 40, 35);
			p.shoeColor = new Color(50, 30, 25);
			for (int i = 0; i < p.armor.Length; i++) p.armor[i] = new Item();
			for (int i = 0; i < p.dye.Length; i++) p.dye[i] = new Item();
			p.armor[0].SetDefaults(ItemID.WizardHat);
			p.dye[0].SetDefaults(ItemID.FlameAndBlackDye);   // black hat with moving flames, not the vanilla Wizard purple
			p.armor[1].SetDefaults(ItemID.RubyRobe);
			p.isFullbright = true;   // sheets are drawn unlit; the game lights the NPC
			p.width = 20;
			p.height = 42;
			return p;
		}
	}
}
