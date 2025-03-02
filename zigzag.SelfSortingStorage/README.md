# Self Sorting Storage

Adds a new unlockable ship ugrade to the game : the **Smart Cupboard**!

This storage container who looks a lot like the vanilla Cupboard furniture, has the effect to automatically sorts equipment items that you store in.

The Smart Cupboard can be acquired by spending 20 credits in the store.

Compatible with v69 of Lethal Company.

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

#### Planned features
- New configs that will allow you to specify in which storage row some items are going to be stored.
- Make a wider version of the Smart Cupboard and other cool things.

##

### Compatibility notes
- [GeneralImprovements](https://thunderstore.io/c/lethal-company/p/ShaosilGaming/GeneralImprovements/) is a required dependency because I'm rotating items based on it's `FixItems` configs. This may change in the future to improve compatibility with other mods.
- Mods that change item resting position and rotation such as [Matty_Fixes](https://thunderstore.io/c/lethal-company/p/mattymatty/Matty_Fixes/) could be a problem. I need some people to help me figure out how these other mods affects items placement in the Smart Cupboard.

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

# Credits

- Thanks [Xu Xiaolan](https://www.youtube.com/shorts/Kt5mnWK-rgI) for helping with some parts of the code, and also for suggesting good things!

- Thanks [A Glitched Npc](https://www.twitch.tv/a_glitched_npc) for the initial idea and for testing!

- Some parts ot the code is based on the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) implementation by [WarperSan](https://thunderstore.io/c/lethal-company/p/WarperSan/), such as the items data structure which has been modified to better fit the SelfSortingStorage mod.

- Cupboard asset is ripped from [Lethal Company](https://store.steampowered.com/app/1966720/Lethal_Company/).

- "vent_chute" by [jazz-the-giraffe](https://sketchfab.com/3d-models/vent-chute-961b5fb81e694f94ab1407028c7dc998) is a free 3D model licensed under Creative Commons Attribution.
