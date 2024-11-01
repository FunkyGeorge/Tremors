using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Lobbies.Models;

public abstract class Puzzle : NetworkBehaviour
{
    [Header("Base Puzzle Config")]
    [SerializeField] protected Sprite unluckySprite;
    [SerializeField] protected Sprite luckySprite;
    protected PuzzleState state = PuzzleState.Waiting;
    protected NetworkVariable<int> serial = new NetworkVariable<int>(-1);

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            InitializePuzzle();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            serial.Value = GameManager.Instance.RegisterPuzzle(gameObject);
        }
    }

    public override void OnDestroy() {
        if (IsServer && NetworkManager.Singleton) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    protected abstract void InitializePuzzle();

    protected virtual void OnClientConnected(ulong clientId) {
        if (IsServer) {
            Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == joinedLobby.Players.Count) {
                serial.Value = GameManager.Instance.RegisterPuzzle(gameObject);
            }
        }
    }

    protected virtual void SetSolved() {
        state = PuzzleState.Solved;
        GetComponent<SpriteRenderer>().sprite = unluckySprite;
        if (IsServer) {
            GameManager.Instance.CheckCompletePuzzle(serial.Value);
        }
    }

    // Called by GameManager when all the puzzles should be solved
    public virtual void SetComplete() {
        state = PuzzleState.Solved;
        GetComponent<SpriteRenderer>().sprite = luckySprite;
    }
}