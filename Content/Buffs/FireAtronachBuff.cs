using Terraria;
using Terraria.ModLoader;
using Conjuration.Content.Projectiles;

namespace Conjuration.Content.Buffs
{
	// The buff is what keeps the minion alive: the minion refreshes its own timeLeft while
	// the owner has the buff, and the buff removes itself when no minion is left.
	public class FireAtronachBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;        // does not survive leaving the world
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<FireAtronachMinion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;   // keep the buff up while the minion lives
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
