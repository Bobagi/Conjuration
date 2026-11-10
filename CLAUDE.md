# Conjuration (tModLoader mod)

Summon-class mod, Fire Atronach first. Read `README.md` (roadmap, how to open) and `docs/ART.md`.

- Language of code and docs: English. No em dash (U+2014) anywhere.
- Build headless: `<tModLoader>/dotnet/dotnet tModLoader.dll -build <repo>` (0 errors on 2026-11-10).
  Tune numbers with `docs/BALANCE.md`.
- Localization: the file name `en-US_Mods.Conjuration.hjson` already sets the `Mods.Conjuration` prefix;
  keys inside start at `Items:`/`Buffs:` (repeating `Mods: { Conjuration:` broke every name once).
- Art: characters are dummy Players drawn with the game's renderer (`Content/Visuals/`); sheets and icons
  are exported in game with `/atronachtest npc|icons`, see `docs/ART.md`. Do not go back to AI generated
  sprites: the owner rejected them (2026-11-10). Verify sizes with `python tools/check_art.py`.
- Playtest: Journey character + `/atronachtest [spawn]`; launch with `-skipselect "player:world"`.
- tModLoader targets .NET 8; the mod uses 1.4.4 APIs (`ModProjectile`, `ModBuff`, `ModItem`).
- Look: the owner wants the creatures and NPCs to look like the Skyrim originals. Start from reference
  images (UESP) and redraw them in Terraria style; never copy Skyrim assets or text.
- Publishing to the Steam Workshop is done by the owner (Gustavo), not by Claude.
