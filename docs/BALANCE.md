# Balance notes

Starting numbers for the Fire Atronach, to be tuned by playing. Reference is vanilla pre-Hardmode
summons around Hellstone tier (Imp Staff: 24 damage, 10 mana, hellstone-era).

| Stat | Value | Where |
|---|---|---|
| Summon damage per fireball | 22 | `Item.damage` in `TomeConjureFireAtronach.cs` |
| Mana to summon | 12 | `Item.mana` |
| Minion slots | 1 | `Projectile.minionSlots` |
| Fireball cooldown | 45 ticks (0.75 s) | `ShootCooldown` in `FireAtronachMinion.cs` |
| Fireball speed | 10 | `FireballSpeed` |
| Range | 700 px from the owner | `DetectRange` |
| Price when sold | 2 gold | `Item.buyPrice` |

Why this is a first guess: a ranged minion that never takes contact damage and cannot die is safe, so it
should be a bit weaker per slot than a melee minion. Fireballs also set enemies on fire (vanilla
`Ball of Fire` projectile), which adds damage over time. If it feels too strong, raise the cooldown first.

## Checklist when testing in single player

- Summon, check the buff icon appears and the minion follows you.
- Two tomes: two minions, they line up behind you instead of stacking.
- It shoots only enemies it can see, prefers the one you right-click (target).
- Death or leaving the world removes it; re-entering does not duplicate it.
- Multiplayer: host a local server and join with a second client; the minion must shoot on both screens.
- Compare DPS against the Imp Staff on the same enemy.
