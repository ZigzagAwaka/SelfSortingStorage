## 1.1.0
- Removed GeneralImprovements of the dependencies in the manifest to help with modpack making. However, an item fixing mod is still required to avoid issues, as stated in the README
- Error messages will now appear in-game when [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) or [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) is not installed, and also, messages will appear if you have wrongly set your mods configs (these message will be displayed on the main menu as well as when purchasing the SSS)
- Updated compatibility notes in the README

## 1.0.9
- Exploded stun grenades are no longer accepted in the storage
- Added `Items Blacklist` config, pre-configured with some items that can cause issues

## 1.0.8
- Modified the cabinet model so that all shelves are now the same size
- Made buttons easier to press
- Fixed the SSS GI Screen being turned off for late join players when there is no item stored
- Updated images and compatibility notes in the README

*[Updating from an older version to 1.0.8+ may cause some item placement issues on already existing saves. To avoid this, you'll need to reset your save, but if you don't want to do that you can instead retrieve every stored items, or press the reset button BEFORE updating.]*

## 1.0.7
- Added the `SSS Screen position` upgrade that can be activated in the config (requires [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) config: `UseBetterMonitors = true`)

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