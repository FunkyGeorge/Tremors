using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PentaPuzzle : Puzzle
{
    [Header("Penta Puzzle Config")]
    [SerializeField] private List<PuzzleNode> nodes = new List<PuzzleNode>();

    protected override void OnClientConnected(ulong clientId) {
        for (int i = 0; i < nodes.Count; i++) {
            SyncNodeClientRPC(i, nodes[i].active);
        }
        base.OnClientConnected(clientId);
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

    protected override void InitializePuzzle() {
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

    protected override void SetSolved() {
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        base.SetSolved();
    }

    // Called by GameManager when all the puzzles should be solved
    public override void SetComplete() {
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        base.SetComplete();
    }
}
