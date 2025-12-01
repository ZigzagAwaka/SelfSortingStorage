## 1.4.5
- Added `Reset Button Host Only` misc config that can be used to make the `Reset Button` only usable by the host
- Fixed christmas reskin having incorect wide stars for the `Wide Cupboard`

## 1.4.4
- Added the `Quantity Cursortip` upgrade that can be activated in the config: will show the quantity of the item stack on the cursortip UI when hovering an item stored in the SSS
- Added [AutoHookGenPatcher](https://thunderstore.io/c/lethal-company/p/Hamunii/AutoHookGenPatcher/) as a dependency
- Added a check to prevent invalid [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) screens from being set by the `SSS Screen position` config

## 1.4.3
- Added `Scraps Whitelist` config, this can be used to force allow specific scraps in the storage when `Allow Scrap items` is disabled
- Updated in-game error messages display to skip some checks if [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) is installed and correctly configured
- Added *something special* for December

## 1.4.2
- Loading of modded items into the cache will now use [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) in priority if it is installed, and [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/) if it is not installed

## 1.4.1
- Fixed compatibility with [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) 0.3.11+

## 1.4.0
- Added compatibility with [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/), and it's now the prefered way to assist this mod as it can replace every other mod's features about fixing item position and rotation
- Added a system where the mod will fallback to using [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/), [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) and [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) if [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) is not installed or if the `Item Saving` config from DawnLib is disabled
- Added the `Permanent items` misc config feature that allows the host player to define specific items that will be permanently stored in the SSS (none by default)
- Added a check to prevent non valid values to being applied from the `Cupboard Color` config
- Fixed a minor error in the SSS memory
- Updated compatibility notes in the README

## 1.3.0
- Updated networking to work for v73 of Lethal Company
- Added [FixPluginTypesSerialization](https://thunderstore.io/c/lethal-company/p/Evaisa/FixPluginTypesSerialization/) as a dependency *(this was needed since 1.0.0 oops)*
- Updated the text displayed when players tries to store an item that is blacklisted
- Updated the text displayed when players tries to store another player's body, a monster or an exploded stun grenade

## 1.2.1
- Added a check to prevent any grabbable enemies to be stored, not just Maneaters

## 1.2.0
- Updated to v70
- Fixed items position in the shelves for v70
- Fixed compatibility and added [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) as a dependency in the mod's manifest file
- Updated compatibility notes in the README

## 1.1.2
- Compatibility patch for [LittleCompany](https://thunderstore.io/c/lethal-company/p/Toybox/LittleCompany/)

## 1.1.1
- Maneater enemies are no longer accepted in the storage

## 1.1.0
- Removed GeneralImprovements of the dependencies in the manifest to help with modpack making. However, an item fixing mod is still required to avoid issues, as stated in the README
- Error messages will now be displayed in-game when [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) or [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) is not installed, and also, messages will appear if you have wrongly set your mods configs (these message will be displayed on the main menu as well as when purchasing the SSS)
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