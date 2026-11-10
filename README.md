# Conjuration · tModLoader mod

Conjuration spells for summoners, inspired by the Atronach spells of the Elder Scrolls games (names and
art are our own, nothing is copied from Bethesda).

**v0.1:** *Tome: Conjure Fire Atronach*. A permanent minion (1 slot) that **walks on the ground** beside you, like the
atronachs of the games that inspired it, and stops to hurl fireballs (the vanilla Imp Fireball, at the
Imp's rate). If it falls far behind it reappears next to you in a burst of fire.

**The Conjurer**, a town NPC modelled on Skyrim's conjuration master, moves in after the Eye of
Cthulhu and sells the tome after Skeletron.

Both are drawn with Terraria's own player renderer (a dressed up dummy player, like the Mannequin),
so they look like the game itself; see `docs/ART.md`.

## Roadmap

1. Fire Atronach minion and tome (done, playtested in single player).
2. The **Conjurer** town NPC that sells the conjuration tomes (done: shop gated by Skeletron).
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
world (or a Journey one for easy testing), craft or spawn the tome, and summon. To jump straight into a
world: `dotnet tModLoader.dll -skipselect "<player>:<world>"` (the player must fit the world's mode).

Headless build (compile check):

```bash
cd "<Steam>/steamapps/common/tModLoader"
./dotnet/dotnet tModLoader.dll -build "<ModSources>/Conjuration"
```

To get the tome fast: `/atronachtest` (Journey characters), or craft it:
Book + 12 Hellstone Bar + 3 Fireblossom at a Bookcase.

## Status

Builds with 0 errors and 0 warnings on tModLoader v2026.9.3.1. Playtested in single player on
2026-11-10: summons on the ground, follows the player walking, stops and throws fireballs at zombies
(17 damage per hit on a Zombie). Multiplayer not tested yet, see `docs/BALANCE.md`.

Playtest shortcut: a **Journey mode** character in single player can type `/atronachtest` in chat to get
the tome, `/atronachtest spawn` to also spawn three zombies, `/atronachtest unlock` (test worlds only:
marks Eye of Cthulhu and Skeletron as beaten) and `/atronachtest conjurer` to meet the NPC.

## Layout

```
Conjuration.cs                        Mod entry point
Content/Items/                        the tome (summon item)
Content/Buffs/                        the buff that keeps the minion alive
Content/Projectiles/                  the minion (AI, fireballs, animation)
Localization/en-US_Mods.Conjuration.hjson   names and tooltips
Content/NPCs/                         the Conjurer town NPC (shop, chat, sheet and head)
Content/Visuals/                      looks of the Atronach and the Conjurer (dummy players)
Content/Commands/                     /atronachtest playtest and sprite export helper (Journey only)
tools/make_atronach_crown.py          draws the Atronach's head piece (mask, horns, plume, plates)
tools/check_art.py                    verifies sprite sizes before building
docs/ART.md                           how the art is made and exported
docs/BALANCE.md                       numbers and the test checklist
```

## Publishing

Same flow as the other mods: *Workshop > Develop Mods > Conjuration > Publish* in game. Page text from
`description.txt`, change notes from `changelog.txt`, icon `icon.png` (80x80), and
`icon_workshop.png` (512x512, add it before publishing). Publishing is the owner's call.

## License

MIT
