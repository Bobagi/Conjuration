# Conjuration (tModLoader mod)

Summon-class mod, Fire Atronach first. Read `README.md` (roadmap, how to open) and `docs/ART.md`.

- Language of code and docs: English. No em dash (U+2014) anywhere.
- Build headless: `<tModLoader>/dotnet/dotnet tModLoader.dll -build <repo>` (0 errors on 2026-11-10).
  Tune numbers with `docs/BALANCE.md`.
- Localization: the file name `en-US_Mods.Conjuration.hjson` already sets the `Mods.Conjuration` prefix;
  keys inside start at `Items:`/`Buffs:` (repeating `Mods: { Conjuration:` broke every name once).
- Art: generated with the local ComfyUI and built by `tools/build_sheet.py`, full pipeline in `docs/ART.md`.
  Verify sizes with `python tools/check_art.py`. Sprites use 2x2 screen pixels per art pixel like vanilla.
- Playtest: Journey character + `/atronachtest [spawn]`; launch with `-skipselect "player:world"`.
- tModLoader targets .NET 8; the mod uses 1.4.4 APIs (`ModProjectile`, `ModBuff`, `ModItem`).
- Do not copy Skyrim assets or text. Original designs only.
- Publishing to the Steam Workshop is done by the owner (Gustavo), not by Claude.
