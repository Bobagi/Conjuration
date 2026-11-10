using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Conjuration.Content.Items;

namespace Conjuration.Content.NPCs
{
	// The Conjurer: a town NPC who sells the conjuration tomes. Moves in once the Eye of Cthulhu is
	// beaten; sells the Fire Atronach tome after Skeletron, the Frost Atronach in Hardmode and the Storm
	// Atronach after any mechanical boss.
	// Sprite sheet and head come from the game's own player renderer (/atronachtest npc), laid out like
	// vanilla town NPCs (25 frames, see Commands/AtronachTestCommand).
	[AutoloadHead]
	public class Conjurer : ModNPC
	{
		public const string ShopName = "Shop";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 25;
			NPCID.Sets.ExtraFramesCount[Type] = 9;     // frames after the walk cycle (16-24)
			NPCID.Sets.AttackFrameCount[Type] = 4;     // 21-24
			NPCID.Sets.DangerDetectRange[Type] = 700;
			NPCID.Sets.AttackType[Type] = 0;           // throws, like the Merchant: here a ball of fire
			NPCID.Sets.AttackTime[Type] = 40;
			NPCID.Sets.AttackAverageChance[Type] = 20;
			NPCID.Sets.HatOffsetY[Type] = 4;           // party hat sits on the wizard hat

			NPC.Happiness
				.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Like)   // quiet places to study
				.SetBiomeAffection<OceanBiome>(AffectionLevel.Dislike)      // too wet for fire magic
				.SetNPCAffection(NPCID.Wizard, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Angler, AffectionLevel.Dislike);
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.Guide;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.Conjuration.NPCs.Conjurer.Bestiary"),
			});
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) => NPC.downedBoss1;

		public override List<string> SetNPCNameList() => new List<string>
		{
			"Aldren", "Corvas", "Merrin", "Galen", "Orsik", "Tobias", "Velan",
		};

		public override string GetChat()
		{
			int n = Main.rand.Next(5);
			if (n == 0 && !NPC.downedBoss3)
				return Language.GetTextValue("Mods.Conjuration.NPCs.Conjurer.Chat.Locked");
			return Language.GetTextValue("Mods.Conjuration.NPCs.Conjurer.Chat.Line" + n);
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28");   // "Shop"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton) shopName = ShopName;
		}

		public override void AddShops()
		{
			new NPCShop(Type, ShopName)
				.Add<TomeConjureFireAtronach>(Condition.DownedSkeletron)
				.Add<TomeConjureFrostAtronach>(Condition.Hardmode)
				.Add<TomeConjureStormAtronach>(Condition.DownedMechBossAny)
				.Register();
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileID.BallofFire;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 10f;
			randomOffset = 1f;
		}
	}
}
