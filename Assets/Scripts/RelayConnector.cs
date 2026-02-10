using System;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class RelayConnector : MonoBehaviour
{
    [SerializeField] private int maxClients = 7; 

    bool _servicesReady;

    async Task EnsureServicesReady()
    {
        if (_servicesReady) return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _servicesReady = true;
    }

    public async Task<string> StartHostWithRelay()
    {
        await EnsureServicesReady();

        // Allocate slots for (maxClients + host)
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxClients);

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, connectionType: "dtls"));

        if (!NetworkManager.Singleton.StartHost())
            throw new Exception("StartHost() failed.");

        return joinCode;
    }

    public async Task StartClientWithRelay(string joinCode)
    {
        await EnsureServicesReady();

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAllocation, connectionType: "dtls"));

        if (!NetworkManager.Singleton.StartClient())
            throw new Exception("StartClient() failed.");
    }
}