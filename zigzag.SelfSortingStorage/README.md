# Self Sorting Storage

### Adds a new unlockable ship ugrade to the game : the **Smart Cupboard**!

This storage container who looks a lot like the vanilla Cupboard furniture, has the effect to automatically sorts equipment items that you store in.

The Smart Cupboard can be acquired by spending 20 credits in the store.

Compatible with v69 of Lethal Company.

> **This mod requires [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) OR [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) installed to avoid some item placement issues.** For more info, check the compatibility notes below.

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

<details><summary>Preview images of the upgrades (click to reveal)</summary>

#### Wide Cupboard

For those who wants more space!

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades1.PNG)

#### Reset Button

Press it to delete everything stored...

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades2.PNG)

#### Cozy Lights

Some decorative lights for the storage!

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades3.PNG)

#### Items list screen

List every stored items on a GeneralImprovements screen.

![Preview](https://raw.githubusercontent.com/ZigzagAwaka/SelfSortingStorage/main/Images/SSS_Upgrades4.png)

</details>

##

### Compatibility notes
- **Required item fixing mods**
    - This mod requires the support of an item fixing mod in order to avoid item placement issues. You have multiple mods that can work but the most effective ones are [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) and [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/).
    - It is recommended to have at least one of these 2 installed before using SelfSortingStorage. However, other similar mods may also work.
- **GeneralImprovements**
    - If you have [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) installed, you need to have `FixItemsLoadingSameRotation` AND `FixItemsFallingThrough` configs enabled. This is needed for items to face the correct direction when stored.
    - The `ShipPlaceablesCollide` config also needs to be enabled to avoid items vanishing when stored.
    - *All of these configs are enabled by default if you haven't change the config file.*
- **Matty_Fixes**
    - If you have [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) installed, you need to have `OutOfBounds.Enabled` config enabled *(which is enabled by default)*.
    - **If you have both GeneralImprovements and Matty_Fixes installed**, then you'll need to disable either `FixItemsFallingThrough` in GeneralImprovements OR disable `OutOfBounds.Enabled` in Matty_Fixes, because these mods do the same thing.
- **Problems buying the Smart Cupboard in the store**
    - This is a common issue for similarly named terminal objects, try installing [TerminalConflictFix](https://thunderstore.io/c/lethal-company/p/SylviBlossom/TerminalConflictFix/) to fix this.
    - Another alternative solution is to use [DarmuhsTerminalStuff](https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/) like this user suggested [here on github](https://github.com/ZigzagAwaka/SelfSortingStorage/issues/2#issuecomment-2708783243).
- **CruiserImproved and custom ships**
    - If you have a custom ship mod AND [CruiserImproved](https://thunderstore.io/c/lethal-company/p/DiggC/CruiserImproved/) installed, reloading a save when the SSS is already unlocked will break it until a complete reset. Please wait for CruiserImproved to be updated to support custom ships.
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

- Thanks [A Glitched Npc](https://www.twitch.tv/a_glitched_npc) for the initial idea and for testing!

- Thanks [ScienceBird](https://thunderstore.io/c/lethal-company/p/ScienceBird/) for the help on the 1.0.4 update!

- Some parts ot the code is based on the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) implementation by [WarperSan](https://thunderstore.io/c/lethal-company/p/WarperSan/), such as the items data structure which has been modified to better fit the SelfSortingStorage mod.

- Cupboard asset is ripped from [Lethal Company](https://store.steampowered.com/app/1966720/Lethal_Company/).

- "vent_chute" by [jazz-the-giraffe](https://sketchfab.com/3d-models/vent-chute-961b5fb81e694f94ab1407028c7dc998) is a free 3D model licensed under Creative Commons Attribution.
