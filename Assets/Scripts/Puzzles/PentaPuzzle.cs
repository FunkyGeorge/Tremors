using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PentaPuzzle : NetworkBehaviour
{

    [SerializeField] private Sprite unluckySprite;
    [SerializeField] private Sprite luckySprite;
    [SerializeField] private List<PuzzleNode> nodes = new List<PuzzleNode>();
    private PuzzleState state = PuzzleState.Waiting;
    private NetworkVariable<int> serial = new NetworkVariable<int>(-1);

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            InitializePuzzle();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            serial.Value = GameManager.Instance.RegisterPuzzle(gameObject);
        }
    }

    void OnClientConnected(ulong clientId) {
        for (int i = 0; i < nodes.Count; i++) {
            SyncNodeClientRPC(i, nodes[i].active);
        }
    }

    public override void OnDestroy() {
        if (IsServer && NetworkManager.Singleton) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].SetIndex(i);
            nodes[i].gameObject.SetActive(false);
        }
    }


    void OnTriggerEnter2D(Collider2D collider) {
        if (!IsServer) { return; }
        if (state == PuzzleState.Waiting) {
            Runner validRunner = collider.gameObject.GetComponent<Runner>();
            if (validRunner) {
                StartPuzzleClientRPC();
            }
        }
    }

    private void InitializePuzzle() {
        // Randomize starting state
        int randomStart = Random.Range(0, nodes.Count);
        for (int i = 0 + randomStart; i < nodes.Count + randomStart; i++) {
            int index = i % nodes.Count;
            bool lucky = Random.value > 0.5;
            SyncNodeClientRPC(index, !(index == randomStart || lucky));
        }
    }

    [ClientRpc]
    private void SyncNodeClientRPC(int index, bool activeState) {
        if (activeState != nodes[index].active) {
            nodes[index].Toggle();
        }
    }

    [ClientRpc]
    private void StartPuzzleClientRPC() {
        foreach (PuzzleNode node in nodes) {
            node.gameObject.SetActive(true);
            state = PuzzleState.Solving;
        }
    }

    public void OnToggleNode(int nodeIndex) {
        if (IsServer) {
            ToggleNodesClientRPC(nodeIndex);
        }
    }

    [ClientRpc]
    private void ToggleNodesClientRPC(int nodeIndex) {
        bool isSolved = true;
        for (int i = 0; i < nodes.Count; i++) {
            if (i == (0 + nodeIndex) % nodes.Count || i == (2 + nodeIndex) % nodes.Count || i == (3 + nodeIndex) % nodes.Count) {
                nodes[i].Toggle();
            }
            isSolved = isSolved && nodes[i].active;
        }

        if (isSolved) {
            SetSolved();
        }
    }

    private void SetSolved() {
        state = PuzzleState.Solved;
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        GetComponent<SpriteRenderer>().sprite = unluckySprite;
        if (IsServer) {
            GameManager.Instance.CheckCompletePuzzle(serial.Value);
        }
    }

    // Called by GameManager when all the puzzles should be solved
    public void SetComplete() {
        state = PuzzleState.Solved;
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        GetComponent<SpriteRenderer>().sprite = luckySprite;
    }
}
