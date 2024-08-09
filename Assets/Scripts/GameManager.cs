using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Team debugStartingTeam;

    [Header("Config")]
    
    private string gameCodeString = "";

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
                            { LobbyManager.KEY_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                        }
                    });
            } catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        try {
            Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
            string relayCode = joinedLobby.Data[LobbyManager.KEY_GAME_CODE].Value;
            string winningTeamString = joinedLobby.Data[LobbyManager.KEY_WINNING_TEAM].Value;

            if (relayCode != gameCodeString) {
                JoinRelayAsClient(relayCode);
            }

            if (winningTeamString != "") {
                ProceedToPostGame();
            }
        } catch (LobbyServiceException err) {
            Debug.Log(err);
        }
    }

    async void JoinRelayAsClient(string relayCode) {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            try {
                JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                        joinAlloc.RelayServer.IpV4,
                        (ushort)joinAlloc.RelayServer.Port,
                        joinAlloc.AllocationIdBytes,
                        joinAlloc.Key,
                        joinAlloc.ConnectionData,
                        joinAlloc.HostConnectionData
                    );
                
                Debug.Log("Joining with relay code: " + relayCode);
                NetworkManager.Singleton.StartClient();
                gameCodeString = relayCode;
            } catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }
    }

    void ProceedToPostGame()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.LoadScene("PostGame", LoadSceneMode.Single);
        }
        // SceneManager.LoadScene("PostGame");
    }

    public void CompleteGame(Team winners) {
        if (LobbyManager.Instance.IsLobbyHost()) {
            LobbyManager.Instance.UpdateLobby(new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { LobbyManager.KEY_WINNING_TEAM, new DataObject(DataObject.VisibilityOptions.Member, winners.ToString()) }
                }
            });
        }
    }
}
