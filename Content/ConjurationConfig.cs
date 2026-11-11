using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Conjuration.Content
{
	// Settings > Mod Configuration > Conjuration.
	public class ConjurationConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// Off by default: /atronachtest gives tomes and can change world progress, so it is a cheat in a
		// normal playthrough. Journey mode characters can always use it (Journey already has free items).
		[DefaultValue(false)]
		public bool AllowTestCommandForAnyCharacter;
	}
}
