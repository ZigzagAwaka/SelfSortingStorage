using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace SelfSortingStorage.Utils
{
    internal static class DestroyItemInSlotMonoPatch
    {
        public static void Load()
        {
            IL.GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot += DestroyItemInUtilitySlot;
        }

        private static void DestroyItemInUtilitySlot(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // Seatch "HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false" and replaces the entire block
            c.GotoNext(
                MoveType.Before,
                x => x.MatchCall<HUDManager>("get_Instance"),
                x => x.MatchLdfld<HUDManager>("itemSlotIcons"),
                x => x.MatchLdarg(1),
                x => x.MatchLdelemRef(),
                x => x.MatchLdcI4(0),
                x => x.MatchCallvirt<UnityEngine.Behaviour>("set_enabled")
            );
            c.RemoveRange(6);  // delete original instructions
            // then replaces it with fixed code
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<int>>(itemSlot =>
            {
                if (itemSlot == 50)
                    HUDManager.Instance.itemOnlySlotIcon.enabled = false;
                else
                    HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
            });
        }
    }
}
