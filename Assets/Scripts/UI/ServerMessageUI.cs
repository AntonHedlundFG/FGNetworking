using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerMessageUI : NetworkBehaviour
{
    private struct DurationMessage
    {
        public string message;
        public float duration;
        public DurationMessage(string message, float duration)
        {
            this.message = message;
            this.duration = duration;
        }
    }
    [SerializeField] private Text _serverMessageText;
    public static ServerMessageUI Instance;

    private Queue<DurationMessage> _messages = new Queue<DurationMessage>();
    public override void OnNetworkSpawn()
    {
        _serverMessageText.text = "";
        Instance = this;
    }

    public void DisplayMessage(string message, float duration)
    {
        if (!IsServer) return;
        DisplayMessageClientRpc(message, duration);
    }

    [ClientRpc]
    private void DisplayMessageClientRpc(string message, float duration)
    {
        DisplayMessageLocally(message, duration);
    }

    private void DisplayMessageLocally(string message, float duration)
    {
        _messages.Enqueue(new DurationMessage(message, duration));
        if (_serverMessageText.text == "")
        {
            StartCoroutine(DisplayMessageRoutine());
        }
    }

    private IEnumerator DisplayMessageRoutine()
    {
        DurationMessage message = _messages.Dequeue();
        _serverMessageText.text = message.message;
        yield return new WaitForSeconds(message.duration);
        _serverMessageText.text = ""; 
        if (_messages.Count > 0)
        {
            StartCoroutine(DisplayMessageRoutine());
        }
    }
}
