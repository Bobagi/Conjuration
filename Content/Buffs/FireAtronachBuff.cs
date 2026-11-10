using Terraria;
using Terraria.ModLoader;
using Conjuration.Content.Projectiles;

namespace Conjuration.Content.Buffs
{
	// The buff keeps the minion alive: the minion refreshes its own timeLeft while the owner has the buff,
	// and the buff removes itself when no minion of its kind is left.
	public abstract class AtronachBuff : ModBuff
	{
		protected abstract int MinionType { get; }

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;        // does not survive leaving the world
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[MinionType] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class FireAtronachBuff : AtronachBuff
	{
		protected override int MinionType => ModContent.ProjectileType<FireAtronachMinion>();
	}

	public class FrostAtronachBuff : AtronachBuff
	{
		protected override int MinionType => ModContent.ProjectileType<FrostAtronachMinion>();
	}

	public class StormAtronachBuff : AtronachBuff
	{
		protected override int MinionType => ModContent.ProjectileType<StormAtronachMinion>();
	}
}
