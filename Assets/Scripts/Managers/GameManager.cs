using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport.Relay;

public class GameManager : NetworkBehaviour
{
    public event EventHandler<int> OnSurvivorsUpdated;
    private List<Transform> tremors = new List<Transform>();
    private NetworkList<NetworkObjectReference> trackedNOs;
    private List<Vector2> runnerPositions = new List<Vector2>();

    // Puzzle Stuff
    [Header("Puzzle Management")]
    [SerializeField] private GameObject keyJar;
    public NetworkVariable<int> winningPuzzleSerial = new NetworkVariable<int>(-1);
    private List<PentaPuzzle> activePuzzles = new List<PentaPuzzle>();

    [Header("Config")]
    [SerializeField] private float gameTimeLimit = 5 * 60;
    private float gameTimeRemaining = -1f;
    
    private string gameCodeString = "";
    private bool isGameActive = false;

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

        trackedNOs = new NetworkList<NetworkObjectReference>();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;

        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        if (joinedLobby == null) {
            await LobbyManager.Instance.Authenticate("Debug");
            await LobbyManager.Instance.CreateLobby("Debug Lobby", 1, true);
        }
        StartRelayHost();
    }

    void Update() {
        CalculateRunnerPositions();

        if (gameTimeRemaining >= 0) {
            gameTimeRemaining -= Time.deltaTime;
            UIManager.Instance.RefreshGameTimer(gameTimeRemaining);
        } else if (isGameActive && gameTimeRemaining <= 0) {
            CompleteGame(Team.SHARK);
        }
    }

    public override void OnDestroy() {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        if (NetworkManager.Singleton) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId) {
        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == joinedLobby.Players.Count) {
            if (IsServer) {
                RollNewWinningSNServerRPC();
            }
            InitializeGameTimeClientRPC(gameTimeLimit);
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    async void StartRelayHost() {
        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();

        if (LobbyManager.Instance.IsLobbyHost()) {
            try {
                    Allocation alloc = await RelayService.Instance.CreateAllocationAsync(joinedLobby.Players.Count);

                    string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(alloc, "wss"));

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

    [ClientRpc]
    void InitializeGameTimeClientRPC(float gameTime) {
        gameTimeRemaining = gameTime;
        isGameActive = true;
    }

    async void JoinRelayAsClient(string relayCode) {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            try {
                JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAlloc, "wss"));
                
                Debug.Log("Joining with relay code: " + relayCode);
                NetworkManager.Singleton.StartClient();
                gameCodeString = relayCode;
            } catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public void CheckActiveRunners() {
        int remaining = GameObject.FindGameObjectsWithTag("Player").Length;
        

        OnSurvivorsUpdated?.Invoke(this, remaining);
        if (remaining == 0) {
            CompleteGame(Team.SHARK);
        }
    }

    public List<Vector2> GetTremorPositions(Vector2 playerPosition) {
        List<Vector2> tremorVectorsOnRadar = new List<Vector2>();
        float minRadarDistance = 5f;
        float maxRadarDistance = 14f;

        if (tremors.Count == 0) {
            foreach(GameObject tremorGO in GameObject.FindGameObjectsWithTag("Tremor")) {
                tremors.Add(tremorGO.transform);
            };
        }

        for (int i = 0; i < tremors.Count; i++) {
            float tremorDistance = Vector2.Distance(playerPosition, tremors[i].position);
            if (tremorDistance > minRadarDistance && tremorDistance < maxRadarDistance) {
                tremorVectorsOnRadar.Add(tremors[i].position);
            }
        }

        return tremorVectorsOnRadar;
    }

    public List<Vector2> GetRunnerPositions() {
        return runnerPositions;
    }

    private void CalculateRunnerPositions() {
        runnerPositions.Clear();
        foreach(NetworkObjectReference NORef in trackedNOs) {
            if (NORef.TryGet(out NetworkObject trackedNO)) {
                runnerPositions.Add(trackedNO.transform.position);
            } else {
                Debug.Log("Tracked Object not found");
            }
        }
    }

    public void HandleTracked(GameObject NO) {
        if (trackedNOs.Contains(NO)) {
            RemoveTrackedObjectServerRPC(NO);
        } else {
            AddTrackedObjectServerRPC(NO);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddTrackedObjectServerRPC(NetworkObjectReference trackedRef) {
        trackedNOs.Add(trackedRef);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTrackedObjectServerRPC(NetworkObjectReference trackedRef) {
        trackedNOs.Remove(trackedRef);
    }

    public int RegisterPuzzle(NetworkObjectReference puzzleRef) {
        int serial = activePuzzles.Count;
        RegisterPuzzleClientRPC(puzzleRef);
        return serial;
    }

    [ClientRpc]
    private void RegisterPuzzleClientRPC(NetworkObjectReference puzzleRef) {
        if (puzzleRef.TryGet(out NetworkObject puzzle)) {
            activePuzzles.Add(puzzle.gameObject.GetComponent<PentaPuzzle>());
        } else {
            Debug.Log("Could not get puzzle");
        }
    }

    [ServerRpc]
    private void RollNewWinningSNServerRPC() {
        winningPuzzleSerial.Value = UnityEngine.Random.Range(0, activePuzzles.Count);
    }

    public void CheckCompletePuzzle(int serial) {
        if (serial == winningPuzzleSerial.Value) {
            SetGamePuzzlesToCompleteClientRPC();
        }
    }

    [ClientRpc]
    private void SetGamePuzzlesToCompleteClientRPC() {
        foreach (PentaPuzzle puzzle in activePuzzles) {
            puzzle.SetComplete();
        }
        Destroy(keyJar);
    }

    void ProceedToPostGame()
    {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.LoadScene("PostGame", LoadSceneMode.Single);
        }
    }

    public void CompleteGame(Team winners) {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsHost) {
            CompleteGameClientRPC(winners);

            LobbyManager.Instance.UpdateLobby(new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { LobbyManager.KEY_WINNING_TEAM, new DataObject(DataObject.VisibilityOptions.Member, winners.ToString()) }
                }
            });
        }
    }

    [ClientRpc]
    private void CompleteGameClientRPC(Team winners) {
        isGameActive = false;
        LobbyManager.Instance.gameWinners = winners;
    }
}
