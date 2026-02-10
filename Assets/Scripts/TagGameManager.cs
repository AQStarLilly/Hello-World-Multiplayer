using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TagGameManager : NetworkBehaviour
{
    public static TagGameManager Instance;

    private List<ulong> players = new List<ulong>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        players.Add(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        players.Remove(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (players.Count < 2)
            return;

        ulong taggerId = players[Random.Range(0, players.Count)];

        var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(taggerId);
        var player = playerObj.GetComponent<TagPlayer>();

        player.SetTagger(true);
    }

}

