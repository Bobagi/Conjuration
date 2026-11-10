# Conjuration · tModLoader mod

Conjuration spells for summoners, inspired by the Atronach spells of the Elder Scrolls games (names and
art are our own, nothing is copied from Bethesda).

**v0.1:** *Tome: Conjure Fire Atronach*. A permanent minion (1 slot) that floats beside you and throws
fireballs at enemies. **The art is placeholder**, see `docs/ART.md`.

## Roadmap

1. Fire Atronach minion and tome (this repo, v0.1, needs art + playtest).
2. A **wizard town NPC** that sells the conjuration tomes (shop with progression conditions).
3. More conjurations: Frost Atronach, Storm Atronach, Dremora.
4. Not planned: the wizard school / portal dimension (needs a custom map and a world-switch system).

## Open the project

Requires tModLoader from Steam (stable, 1.4.4). Clone **into the ModSources folder**:

```bash
# macOS
cd "$HOME/Library/Application Support/Terraria/tModLoader/ModSources"
# Windows: %USERPROFILE%\Documents\My Games\Terraria\tModLoader\ModSources
git clone https://github.com/Bobagi/Conjuration.git
```

Then in game: *Workshop > Develop Mods > Conjuration > Build + Reload*. Make a new single player
world (or a Journey one for easy testing), craft or spawn the tome, and summon.

Headless build (compile check):

```bash
cd "<Steam>/steamapps/common/tModLoader"
./dotnet/dotnet tModLoader.dll -build "<ModSources>/Conjuration"
```

To get the tome fast: cheat it in with the *Cheat Sheet* / *HERO's Mod* mods, or craft it:
Book + 12 Hellstone Bar + 3 Fireblossom at a Bookcase.

## Status

The code was written **without being compiled or run** (written on a machine without the game window).
Expect to fix small API errors on the first build. Report or fix them in `Content/`.

## Layout

```
Conjuration.cs                        Mod entry point
Content/Items/                        the tome (summon item)
Content/Buffs/                        the buff that keeps the minion alive
Content/Projectiles/                  the minion (AI, fireballs, animation)
Localization/en-US_Mods.Conjuration.hjson   names and tooltips
tools/make_placeholders.py            generates the placeholder PNGs
tools/check_art.py                    verifies sprite sizes before building
docs/ART.md                           sprite specs + ComfyUI workflow
docs/BALANCE.md                       numbers and the test checklist
```

## Publishing

Same flow as the other mods: *Workshop > Develop Mods > Conjuration > Publish* in game. Page text from
`description.txt`, change notes from `changelog.txt`, icon `icon.png` (80x80), and
`icon_workshop.png` (512x512, add it before publishing). Publishing is the owner's call.

## License

MIT
