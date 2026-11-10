using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Items;

namespace Conjuration.Content.Commands
{
	// Playtest helper, only for Journey mode characters in single player:
	//   /atronachtest          gives the tome
	//   /atronachtest spawn    also spawns a few zombies to shoot at
	public class AtronachTestCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "atronachtest";
		public override string Usage => "/atronachtest [spawn]";
		public override string Description => "Journey mode only: gives the Fire Atronach tome (and spawns test zombies).";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player p = caller.Player;
			if (Main.netMode != NetmodeID.SinglePlayer || p.difficulty != PlayerDifficultyID.Creative)
			{
				caller.Reply("Only for Journey mode characters in single player.", Color.OrangeRed);
				return;
			}
			p.QuickSpawnItem(p.GetSource_GiftOrReward(), ModContent.ItemType<TomeConjureFireAtronach>());
			if (args.Length > 0 && args[0] == "spawn")
			{
				for (int i = 0; i < 3; i++)
					NPC.NewNPC(p.GetSource_FromThis(), (int)p.Center.X + 300 + i * 60, (int)p.Center.Y - 40, NPCID.Zombie);
			}
			caller.Reply("Fire Atronach test kit given.", Color.Orange);
		}
	}
}
