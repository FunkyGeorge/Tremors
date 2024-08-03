using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Team debugStartingTeam;

    [Header("Config")]
    


    private static GameManager _instance;
    public static GameManager Instance
    {
        get {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;

        // Players should be signed in at this point
        StartRelayHost();
    }

    private void OnDestroy() {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
    }

    async void StartRelayHost() {
        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        Debug.Log("Tremors: " + joinedLobby.Data[LobbyManager.KEY_TREMOR_IDS].Value);

        if (LobbyManager.Instance.IsLobbyHost()) {
            try {
                    Allocation alloc = await RelayService.Instance.CreateAllocationAsync(joinedLobby.Players.Count);

                    string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                        alloc.RelayServer.IpV4,
                        (ushort)alloc.RelayServer.Port,
                        alloc.AllocationIdBytes,
                        alloc.Key,
                        alloc.ConnectionData
                    );

                    NetworkManager.Singleton.StartHost();

                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                        Data = new Dictionary<string, DataObject> {
                            { LobbyManager.GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                        }
                    });
            } catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }

    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        JoinRelayAsClient();
    }

    async void JoinRelayAsClient() {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            try {
                Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
                string relayCode = joinedLobby.Data[LobbyManager.GAME_CODE].Value;

                if (relayCode == "") { return; }

                JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                        joinAlloc.RelayServer.IpV4,
                        (ushort)joinAlloc.RelayServer.Port,
                        joinAlloc.AllocationIdBytes,
                        joinAlloc.Key,
                        joinAlloc.ConnectionData,
                        joinAlloc.HostConnectionData
                    );

                NetworkManager.Singleton.StartClient();
            } catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
