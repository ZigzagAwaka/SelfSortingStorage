using GameNetcodeStuff;
using Unity.Netcode;

namespace SelfSortingStorage
{
    public class SmartCupboard : NetworkBehaviour
    {
        public InteractTrigger triggerScript;

        public SmartCupboard() { }

        public void Update()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                var player = GameNetworkManager.Instance.localPlayerController;
                triggerScript.interactable = player.isHoldingObject && !player.isGrabbingObjectAnimation && player.currentlyHeldObjectServer != null;
            }
        }

        public void StoreObject(PlayerControllerB playerWhoTriggered)
        {
            if (!playerWhoTriggered.isHoldingObject || playerWhoTriggered.isGrabbingObjectAnimation || playerWhoTriggered.currentlyHeldObjectServer == null)
            {
                return;
            }

        }
    }
}
