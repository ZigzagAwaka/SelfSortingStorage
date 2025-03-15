## 1.0.7
- Added the `SSS Screen position` upgrade that can be activated in the config

## 1.0.6
- Fixed some items having a desynced position and rotation for late join players
- Fixed a network error for client players when the SSS is spawned
- Tweaked the scaling algorithm to perfectly rescale items based on their collider max size. The old scaling algorithm can still be re-activated in the config

## 1.0.5
- Added the `Reset Button` and `Cozy Lights` upgrades that can be activated in the config
- Added a scan node to the storage box, can be removed in the config

## 1.0.4
- Smart Cupboard is now parented to the ship when spawned, this change allows the following:
    - Fixed items retrieved from the storage while the ship is moving to "jitter" in player hands
    - Stored items will now be recognized by mods like [ShipLoot](https://thunderstore.io/c/lethal-company/p/tinyhoot/ShipLoot/)
- Updated compatibility notes in the README

## 1.0.3
- Added the S4 Upgrade `Wide Cupboard` that can be activated in the config

## 1.0.2
- Added a new config `Rows order` that allows to customize the order of items placement in the storage

## 1.0.1
- Fixed items sometimes floating above shelves if they are rescaled
- Placed items will now be correctly parented to the cupboard upon reloading the lobby, and same thing for late join players

## 1.0.0
- Initial release