using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Conjuration.Content.Items;
using Conjuration.Content.Visuals;

namespace Conjuration.Content.Commands
{
	// Playtest and art helper, only for Journey mode characters in single player:
	//   /atronachtest            puts the three atronach tomes in hotbar slots 7, 8 and 9
	//   /atronachtest spawn      also spawns a few zombies to fight
	//   /atronachtest unlock     marks Eye of Cthulhu, Skeletron, Hardmode and a mech boss as done (test worlds only!)
	//   /atronachtest conjurer   spawns the Conjurer next to you (to test the shop)
	//   /atronachtest hairsm     renders the Conjurer with every hair style (to pick one)
	//   /atronachtest npc        renders the Conjurer town NPC sheet (25 frames) and head icon
	// Exports land in <Terraria save folder>/ConjurationExport.
	public class AtronachTestCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "atronachtest";
		public override string Usage => "/atronachtest [spawn|unlock|conjurer|hairsm|npc]";
		public override string Description => "Journey mode only: atronach playtest kit and Conjurer art exports.";

		public static string ExportDir => Path.Combine(Main.SavePath, "ConjurationExport");

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player p = caller.Player;
			if (Main.netMode != NetmodeID.SinglePlayer || p.difficulty != PlayerDifficultyID.Creative)
			{
				caller.Reply("Only for Journey mode characters in single player.", Color.OrangeRed);
				return;
			}
			string sub = args.Length > 0 ? args[0] : "";
			if (sub == "hairsm") { caller.Reply("Wrote " + ExportHairs("hairs_conjurer.png", ConjurerLook.Create), Color.Orange); return; }
			if (sub == "npc") { caller.Reply("Wrote " + ExportConjurer(), Color.Orange); return; }
			if (sub == "unlock")
			{
				// test world only: the Conjurer moves in and sells every tome
				NPC.downedBoss1 = NPC.downedBoss3 = NPC.downedMechBoss1 = NPC.downedMechBossAny = true;
				Main.hardMode = true;
				caller.Reply("Eye of Cthulhu, Skeletron, Hardmode and a mechanical boss marked as done in this world.", Color.Orange);
				return;
			}
			if (sub == "conjurer")
			{
				NPC.NewNPC(p.GetSource_FromThis(), (int)p.Center.X + 60, (int)p.Bottom.Y, ModContent.NPCType<NPCs.Conjurer>());
				caller.Reply("Conjurer spawned.", Color.Orange);
				return;
			}

			// straight into hotbar slots 7, 8 and 9 (replacing what was there: test characters only)
			int[] tomes = { ModContent.ItemType<TomeConjureFireAtronach>(), ModContent.ItemType<TomeConjureFrostAtronach>(), ModContent.ItemType<TomeConjureStormAtronach>() };
			for (int i = 0; i < tomes.Length; i++)
				p.inventory[6 + i].SetDefaults(tomes[i]);
			if (sub == "spawn")
			{
				for (int i = 0; i < 4; i++)
					NPC.NewNPC(p.GetSource_FromThis(), (int)p.Center.X + 300 + i * 60, (int)p.Center.Y - 40, NPCID.Zombie);
			}
			caller.Reply("Atronach tomes given.", Color.Orange);
		}

		// Renders with the game's sprite batch into an off screen target. The batch maps to the back buffer,
		// so the target is screen sized (w x h must fit on screen) and the top left w x h is read back.
		// draw receives the top left of the target in world coordinates (renderers subtract screenPosition).
		public static Color[] RenderToData(int w, int h, Action<Vector2> draw)
		{
			GraphicsDevice gd = Main.instance.GraphicsDevice;
			int sw = gd.PresentationParameters.BackBufferWidth, sh = gd.PresentationParameters.BackBufferHeight;
			if (w > sw || h > sh) throw new ArgumentException($"{w}x{h} does not fit the {sw}x{sh} screen");
			using var rt = new RenderTarget2D(gd, sw, sh);
			RenderTargetBinding[] old = gd.GetRenderTargets();
			gd.SetRenderTarget(rt);
			gd.Clear(Color.Transparent);
			// Immediate: armor dye shaders are applied per draw call, like when the game draws players.
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			draw(Main.screenPosition);
			Main.spriteBatch.End();
			gd.SetRenderTargets(old);
			var data = new Color[w * h];
			rt.GetData(0, new Rectangle(0, 0, w, h), data, 0, data.Length);
			return data;
		}

		public static string SavePng(string name, Color[] data, int w, int h, bool trim = false)
		{
			if (trim) data = Trim(data, ref w, ref h);
			Directory.CreateDirectory(ExportDir);
			string path = Path.Combine(ExportDir, name);
			using var tex = new Texture2D(Main.instance.GraphicsDevice, w, h);
			tex.SetData(data);
			using (FileStream fs = File.Create(path)) tex.SaveAsPng(fs, w, h);
			return path;
		}

		public static string RenderToPng(string name, int w, int h, Action<Vector2> draw, bool trim = false)
			=> SavePng(name, RenderToData(w, h, draw), w, h, trim);

		private static Color[] Trim(Color[] data, ref int w, ref int h)
		{
			int x0 = w, y0 = h, x1 = -1, y1 = -1;
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
					if (data[y * w + x].A > 0) { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
			if (x1 < 0) return data;
			int nw = x1 - x0 + 1, nh = y1 - y0 + 1;
			var outData = new Color[nw * nh];
			for (int y = 0; y < nh; y++)
				for (int x = 0; x < nw; x++)
					outData[y * nw + x] = data[(y + y0) * w + x + x0];
			w = nw;
			h = nh;
			return outData;
		}

		// Town NPC sheet layout (Rijam's Town NPC guide): 0 idle, 1 jump, 2-15 walk, 16-17 raised arm,
		// 18 sit, 19 talk, 20 blink, 21-24 throw attack. Each frame is a player body/legs frame pair.
		public const int NpcFrameW = 40, NpcFrameH = 58, NpcFrames = 25;
		private static readonly (int body, int legs)[] NpcFrameMap = BuildNpcFrameMap();

		private static (int, int)[] BuildNpcFrameMap()
		{
			var m = new (int, int)[NpcFrames];
			m[0] = (0, 0);
			m[1] = (5, 5);
			for (int i = 0; i < 14; i++)
			{
				int walk = 7 + i % 13;   // the player walk cycle is legs 7..19 (13 frames); 14th repeats the first
				m[2 + i] = (walk, walk);
			}
			m[16] = (2, 0);
			m[17] = (3, 0);
			m[18] = (0, 5);
			m[19] = (0, 0);
			m[20] = (0, 0);
			m[21] = (1, 0);
			m[22] = (2, 0);
			m[23] = (3, 0);
			m[24] = (4, 0);
			return m;
		}

		// A player frame (40x56) is drawn 10 px left of and 10 px above the 20x42 hitbox position.
		private static void DrawFrame(Player d, Vector2 frameTopLeft, int body, int legs)
		{
			d.position = frameTopLeft + new Vector2(10f, 10f);
			d.velocity = Vector2.Zero;
			d.ResetEffects();
			d.ResetVisibleAccessories();
			d.UpdateDyes();
			d.UpdateSocialShadow();
			d.PlayerFrame();
			d.bodyFrame.Y = body * d.bodyFrame.Height;
			d.legFrame.Y = legs * d.legFrame.Height;
			Main.PlayerRenderer.DrawPlayer(Main.Camera, d, d.position, 0f, d.fullRotationOrigin);
		}

		// Frame by frame (the whole sheet is taller than the screen), unlit like vanilla sheets.
		private static string ExportConjurer()
		{
			var sheet = new Color[NpcFrameW * NpcFrameH * NpcFrames];
			RenderToData(NpcFrameW, NpcFrameH, origin => DrawFrame(ConjurerLook.Create(), origin, 0, 0));   // warm up: the first render comes out scaled
			for (int i = 0; i < NpcFrames; i++)
			{
				int f = i;
				Color[] frame = RenderToData(NpcFrameW, NpcFrameH, origin =>
				{
					Player d = ConjurerLook.Create();
					d.direction = -1;   // town NPC sheets face left
					DrawFrame(d, origin, NpcFrameMap[f].body, NpcFrameMap[f].legs);
				});
				Array.Copy(frame, 0, sheet, i * frame.Length, frame.Length);
			}
			string path = SavePng("Conjurer.png", sheet, NpcFrameW, NpcFrameH * NpcFrames);
			RenderToPng("Conjurer_Head.png", NpcFrameW, 30, origin =>
			{
				Player d = ConjurerLook.Create();
				d.direction = -1;
				DrawFrame(d, origin, 0, 0);
			}, trim: true);
			return path;
		}

		private static string ExportHairs(string name, Func<Player> make)
		{
			const int cols = 15, cw = 44, ch = 60, rowsPerChunk = 10;
			int count = HairID.Count;
			int rows = (count + cols - 1) / cols;
			var all = new Color[cols * cw * rows * ch];
			for (int r0 = 0; r0 < rows; r0 += rowsPerChunk)
			{
				int nr = Math.Min(rowsPerChunk, rows - r0), start = r0;
				Color[] part = RenderToData(cols * cw, nr * ch, origin =>
				{
					for (int i = start * cols; i < Math.Min(count, (start + nr) * cols); i++)
					{
						Player d = make();
						d.hair = i;
						d.direction = 1;
						d.position = origin + new Vector2((i % cols) * cw + 12, (i / cols - start) * ch + 10);
						d.ResetEffects();
						d.ResetVisibleAccessories();
						d.UpdateDyes();
						d.PlayerFrame();
						Main.PlayerRenderer.DrawPlayer(Main.Camera, d, d.position, 0f, d.fullRotationOrigin);
					}
				});
				Array.Copy(part, 0, all, start * ch * cols * cw, part.Length);
			}
			return SavePng(name, all, cols * cw, rows * ch);
		}
	}
}
