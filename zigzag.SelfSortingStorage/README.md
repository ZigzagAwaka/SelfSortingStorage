# Self Sorting Storage

### Adds a new unlockable ship ugrade to the game : the **Smart Cupboard**!

This storage container who looks a lot like the vanilla Cupboard furniture, has the effect to automatically sorts equipment items that you store in.

The Smart Cupboard can be acquired by spending 20 credits in the store.

Compatible with v73 of Lethal Company.

> **This mod requires [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) installed to avoid some item placement issues. However it is completely possible to not have it installed and instead rely on [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/) and [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) if wanted. [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) can also be installed to improve item rotations.** For more info, check the compatibility notes below.

##

### How to use
On the side of this special Cupboard, you can find a storage box where players can dump their tools like flashlights, walkie-talkies, shovels, spray paints and even scrap items.

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Preview2.PNG)

The storage will then sort these tools and put them on display in specific sections. Then all items will be organized and ready for players to grab and go!

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Preview1.PNG)

### Multiple copies of one item
- If you dump multiples copies of the same item, the storage will have them all 'visually' stacked on top of each other.
- In vanilla, having a lot of items in the ship can sometimes cause some lags. But with this mod installed, all subsequent copies of the same item are going to be stored in the Cupboard's memory. So, you will no longer have lag issues for buying 30+ shovels!

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Preview3.gif)

### Other features
- All items stored in the Smart Cupboard are saved in the host player's current save file.
- If you dump a big item in the box, it will be rescaled to fit nicely in the storage!
- You can customize a lot of things in the config file (such as the price and the color of the cupboard).
- Some **optional upgrades** can be activated in the configs.

**<details><summary>Preview images of the upgrades (click to reveal)</summary>**

### Wide Cupboard

For those who wants more space!

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades1.PNG)

### Reset Button

Press it to delete everything stored... and reset the Cupboard's memory.

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades2.PNG)

### Cozy Lights

Some decorative lights for the storage! (with a button!)

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades3.PNG)

### Items list screen

List every stored items on a [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) screen (cycles through all shelves).

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades4.png)

### Quantity display

Show the quantity of the item stack on the cursortip UI when hovering a stored item.

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades5.PNG)


</details>

##

### Compatibility notes
- **Used modding library**
    - This mod is best to be used with [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) as **it will save items positions and rotations** by default.
    - However, you can safely remove DawnLib if you want and install [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/) instead but items positions will not be saved anymore and so you'll need to install another mod as stated just below.
- **Required item fixing mods**
    - If [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) is installed with the `Item Saving` config set to false (so the system is enabled) then you are good to go and you don't need anything else.
    - If not, then this mod will require the support of an item fixing mod in order to avoid item placement issues. You have multiple mods that can work but the most effective ones are [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) and [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/).
    - **Matty_Fixes will be used to prevent items from falling through the shelves, and GeneralImprovements will be used to correctly save items rotations.**
    - If you are in this case, then it is recommended to have at least Matty_Fixes installed before using SelfSortingStorage. However, other similar mods may also work.
- **Matty_Fixes**
    - If you have [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) installed, you need to have `OutOfBounds.Enabled` config enabled *(which is enabled by default)*.
    - **If you have both GeneralImprovements and Matty_Fixes installed**, then you'll need to disable either `FixItemsFallingThrough` in GeneralImprovements OR disable `OutOfBounds.Enabled` in Matty_Fixes, because these mods do the same thing.
- **GeneralImprovements**
    - If you have [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) installed, you need to have `FixItemsLoadingSameRotation` AND `FixItemsFallingThrough` configs enabled ; this is needed for items to face the correct direction when stored and when loaded (this is NOT needed if [DawnLib](https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/) is also installed).
    - The `ShipPlaceablesCollide` config also needs to be enabled to avoid items vanishing when stored (this is needed all the time).
    - *All of these configs are enabled by default if you haven't change the config file.*
- **Problems buying the Smart Cupboard in the store**
    - This is a common issue for similarly named terminal objects, try deleting the save or installing [TerminalConflictFix](https://thunderstore.io/c/lethal-company/p/SylviBlossom/TerminalConflictFix/) to fix this.
    - Another alternative solution is to use [DarmuhsTerminalStuff](https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/) like this user suggested [here on github](https://github.com/ZigzagAwaka/SelfSortingStorage/issues/2#issuecomment-2708783243).
- **CruiserImproved and custom ships**
    - If you have a custom ship mod AND [CruiserImproved](https://thunderstore.io/c/lethal-company/p/DiggC/CruiserImproved/) installed, reloading a save when the SSS is already unlocked may break it until a complete reset. Please wait for CruiserImproved to be updated to support custom ships.
- **About forced moving items mods and scraps keeping mods**
    - Mods that can force move items such as SellMyScrap when selling at the Company are not compatible (can be probably tweaked to avoid issues).
    - Mods that changes how the scraps are kept on a team wipe or a game over are NOT compatible. The only one that is compatible is the "Selective scrap keeping" feature in [ScienceBird_Tweaks](https://thunderstore.io/c/lethal-company/p/ScienceBird/ScienceBird_Tweaks/).
- **TerminalFormatter item listing**
    - If you want stored items to be listed in the "owned" section of the shop when using [TerminalFormatter](https://thunderstore.io/c/lethal-company/p/mrov/TerminalFormatter/), then you'll need to install [ScienceBird_Tweaks](https://thunderstore.io/c/lethal-company/p/ScienceBird/ScienceBird_Tweaks/) !

##

### How to add custom item condition
**For items developers!** By default, every items are allowed in the storage but if you want to add a custom "condition" to your scraps and tools, so the Smart Cupboard will not accept them, you can do so easily by adding this mod as a soft dependency and then write this code :

```cs
public static void AddValidation()
{
    SelfSortingStorage.Cupboard.SmartCupboard.AddTriggerValidation(MyCustomValidation, "[Your custom message]");
    // the custom message is displayed when a player tries to store something checked by your condition
}

private static bool MyCustomValidation(PlayerControllerB player)
{
    var item = player.currentlyHeldObjectServer;
    if (item is MyCustomItem)  // check your item
        return false;  // don't store the item
    return true;  // allow the item to be stored
}
```

##

### Contact & Feedback
If you want to suggest new features or contact me please go to the mod release page in the [modding discord](https://discord.gg/XeyYqRdRGC) or as a [github issue](https://github.com/ZigzagAwaka/SelfSortingStorage).

###

##

## Credits

- Thanks [Xu Xiaolan](https://www.youtube.com/shorts/Kt5mnWK-rgI) for helping with some parts of the code, and also for suggesting good things!

- Thanks [A Glitched Npc](https://www.twitch.tv/a_glitched_npc) for the initial mod idea and for testing!

- Thanks [Matty](https://thunderstore.io/c/lethal-company/p/mattymatty/) for helping with [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/)  compatibility and other general suggestions.

- Thanks [ScienceBird](https://thunderstore.io/c/lethal-company/p/ScienceBird/) for the help concerning furnitures parenting!

- Some parts ot the code is based on the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) implementation by [WarperSan](https://thunderstore.io/c/lethal-company/p/WarperSan/), such as the items data structure which has been modified to better fit the SelfSortingStorage mod.

- Cupboard asset is ripped and edited from [Lethal Company](https://store.steampowered.com/app/1966720/Lethal_Company/).

- "vent_chute" by [jazz-the-giraffe](https://sketchfab.com/3d-models/vent-chute-961b5fb81e694f94ab1407028c7dc998) is a free 3D model licensed under Creative Commons Attribution.
