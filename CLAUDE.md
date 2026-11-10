# Conjuration (tModLoader mod)

Summon-class mod, Fire Atronach first. Read `README.md` (roadmap, how to open) and `docs/ART.md`.

- Language of code and docs: English. No em dash (U+2014) anywhere.
- The code was written without compiling. First job on a machine with the game: build, fix API errors,
  playtest in single player, then tune with `docs/BALANCE.md`.
- Art: placeholders from `tools/make_placeholders.py`. Real art goes in the same paths and sizes;
  verify with `python3 tools/check_art.py`. Never run the placeholder script again without `--keep-existing`.
- tModLoader targets .NET 8; the mod uses 1.4.4 APIs (`ModProjectile`, `ModBuff`, `ModItem`).
- Do not copy Skyrim assets or text. Original designs only.
- Publishing to the Steam Workshop is done by the owner (Gustavo), not by Claude.
