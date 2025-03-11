using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SelfSortingStorage.Buttons
{
    public class LightButton : NetworkBehaviour
    {
        public MeshRenderer[] bulbs;
        public Light[] lights;
        private bool isOn = false;
        private Color originalBulbColor;

        public LightButton() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!Plugin.config.cozyLights.Value)
                return;
            originalBulbColor = bulbs[0].materials[0].GetColor("_EmissiveColor");
            SwitchLights(false);
            if (!IsServer)
                StartCoroutine(SyncButton());
        }

        public void Press(bool _)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
                return;
            isOn = !isOn;
            SwitchLights(isOn);
        }

        private void SwitchLights(bool on)
        {
            var color = on ? originalBulbColor : Color.black;
            foreach (var bulb in bulbs)
            {
                bulb.materials[0].SetColor("_EmissiveColor", color);
            }
            foreach (var light in lights)
            {
                light.enabled = on;
            }
        }

        private IEnumerator SyncButton()
        {
            while (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
                yield return new WaitForSeconds(0.03f);
            SyncServerRpc(GameNetworkManager.Instance.localPlayerController.OwnerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SyncServerRpc(ulong clientId)
        {
            if (!isOn)
                return;
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            SyncClientRpc(isOn, clientRpcParams);
        }

        [ClientRpc]
        private void SyncClientRpc(bool on, ClientRpcParams clientRpcParams = default)
        {
            isOn = on;
            SwitchLights(isOn);
        }
    }
}
