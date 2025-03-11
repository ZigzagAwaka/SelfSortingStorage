using SelfSortingStorage.Cupboard;
using SelfSortingStorage.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SelfSortingStorage.Buttons
{
    public class DeathButton : NetworkBehaviour
    {
        public InteractTrigger triggerScript;
        public Transform explosionPosition;
        public AudioSource buttonAudio;
        public AudioClip buttonAlarm;
        public SmartCupboard smartCupboardScript;
        private readonly float alarmTime = 5.5f;
        private float actualTime = 0f;
        private bool inAlarmPhase = false;
        private bool isAboutToClear = false;

        public DeathButton() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer)
                StartCoroutine(SyncButton());
        }

        public void Press(bool _)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || isAboutToClear)
                return;
            if (inAlarmPhase)
            {
                isAboutToClear = true;
                inAlarmPhase = false;
                actualTime = 0f;
                triggerScript.hoverTip = "";
                StartCoroutine(ClearSSS());
            }
            else
            {
                inAlarmPhase = true;
                buttonAudio.PlayOneShot(buttonAlarm, 1.5f);
                triggerScript.hoverTip = "Confirm ? : [LMB]";
            }
        }

        private IEnumerator ClearSSS()
        {
            yield return new WaitForSeconds(0.2f);
            yield return Effects.FadeOutAudio(buttonAudio, 0.1f, true);
            yield return new WaitForSeconds(1.2f);
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
                yield break;
            var explosion = Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition.position, Quaternion.Euler(-90f, 0f, 0f));
            explosion.SetActive(value: true);
            triggerScript.hoverTip = "Reset : [LMB]";
            smartCupboardScript.memory.Size = 0;
            isAboutToClear = false;
            if (IsServer)
            {
                foreach (var (_, item) in smartCupboardScript.placedItems)
                {
                    var component = item.gameObject.GetComponent<NetworkObject>();
                    if (component != null && component.IsSpawned)
                    {
                        component.Despawn();
                    }
                }
                smartCupboardScript.memory.ClearAll();
                smartCupboardScript.placedItems.Clear();
                if (Plugin.config.verboseLogging.Value)
                    Plugin.logger.LogInfo("Smart Cupboard was reseted because of the Death Button.");
            }
        }

        public void Update()
        {
            if (inAlarmPhase)
            {
                actualTime += Time.deltaTime;
                if (actualTime >= alarmTime)
                {
                    inAlarmPhase = false;
                    actualTime = 0f;
                    triggerScript.hoverTip = "Reset : [LMB]";
                }
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
            if (!isAboutToClear && !inAlarmPhase)
                return;
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            SyncClientRpc(isAboutToClear, inAlarmPhase, actualTime, triggerScript.hoverTip, clientRpcParams);
        }

        [ClientRpc]
        private void SyncClientRpc(bool aboutToClear, bool alarmPhase, float time, string tip, ClientRpcParams clientRpcParams = default)
        {
            isAboutToClear = aboutToClear;
            inAlarmPhase = alarmPhase;
            actualTime = time;
            triggerScript.hoverTip = tip;
        }
    }
}
