using GameNetcodeStuff;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SelfSortingStorage.Cupboard;
using System;
using UnityEngine.UI;

namespace SelfSortingStorage.Utils
{
    internal static class CursortipMonoPatch
    {
        public static void Load()
        {
            IL.GameNetcodeStuff.PlayerControllerB.SetHoverTipAndCurrentInteractTrigger += DisplayQuantityOnCursortip;
        }

        private static void DisplayQuantityOnCursortip(ILContext il)
        {
            var c = new ILCursor(il);
            // Seatch "cursorIcon.sprite = grabItemIcon" and inject after it
            c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.cursorIcon)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.grabItemIcon)),
                x => x.MatchCallvirt<Image>("set_" + nameof(Image.sprite))
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<PlayerControllerB>>((self) =>
            {
                if (self == null || !SmartCupboard.SpawnedInShip || SmartCupboard.StoredInstanceQuantities.Count == 0)
                    return;
                var item = self.hit.collider.gameObject.GetComponent<GrabbableObject>();
                if (item == null || !SmartCupboard.StoredInstanceQuantities.ContainsKey(item.NetworkObjectId))
                    return;
                self.cursorTip.text += $" ({SmartCupboard.StoredInstanceQuantities[item.NetworkObjectId]})";
            });
        }
    }
}
