# Conjuration (tModLoader mod)

Summon-class mod: Flame, Frost and Storm Atronachs and the Conjurer town NPC who sells their tomes. Read `README.md` (roadmap, how to open) and `docs/ART.md`.

- Language of code and docs: English. No em dash (U+2014) anywhere.
- Build headless: `<tModLoader>/dotnet/dotnet tModLoader.dll -build <repo>` (0 errors on 2026-11-10).
  Tune numbers with `docs/BALANCE.md`.
- Localization: the file name `en-US_Mods.Conjuration.hjson` already sets the `Mods.Conjuration` prefix;
  keys inside start at `Items:`/`Buffs:` (repeating `Mods: { Conjuration:` broke every name once).
- Art (docs/ART.md): atronach sheets come from the Skyrim reference images via Qwen-Image-Edit poses and
  `tools/atronach_art.py` (owner's call, 2026-11-11: use the real references). The Conjurer is a dummy
  Player exported with `/atronachtest npc`. Hand edits live in `art/` (1x, one PNG per frame):
  `python tools/art_folder.py pack` builds Content/; the owner edits the art himself. Verify sizes with
  `python tools/check_art.py`.
- Playtest: Journey character + `/atronachtest [spawn|unlock|conjurer]`; launch with
  `-skipselect "player:world"` (test pair on this PC: `atronachtester:conjurationtest`).
- tModLoader targets .NET 8; the mod uses 1.4.4 APIs (`ModProjectile`, `ModBuff`, `ModItem`).
- Look: the owner wants the creatures and NPCs to look like the Skyrim originals. Start from reference
  images (UESP) and redraw them in Terraria style; never copy Skyrim assets or text.
- Publishing to the Steam Workshop is done by the owner (Gustavo), not by Claude.
