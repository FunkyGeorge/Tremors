using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject runnerPrefab;
    [SerializeField] private string runnerSpawnTag;
    [SerializeField] private GameObject sharkPrefab;
    [SerializeField] private string sharkSpawnTag;

    [Header("Debug")]
    [SerializeField] private Team debugStartingTeam;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnDestroy() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    void OnClientConnected(ulong clientId) {
        Lobby currentLobby = LobbyManager.Instance.GetJoinedLobby();
        if (NetworkManager.Singleton.LocalClientId == clientId && currentLobby != null) {
            LobbyManager.Instance.AssignPlayerConnectionId(clientId);

            string[] tremorIds = currentLobby.Data[LobbyManager.KEY_TREMOR_IDS].Value.Split("_");

            if (currentLobby.Players.Count == 1) {
                // If there is no lobby, This should happen only for debugging when starting
                // in the desert scene
                SpawnPlayerServerRPC(clientId, debugStartingTeam);
            } else if (tremorIds.Contains(AuthenticationService.Instance.PlayerId)) {
                SpawnPlayerServerRPC(clientId, Team.SHARK);
            } else {
                SpawnPlayerServerRPC(clientId, Team.RUNNER);
            }
        }
    }

    public void SpawnPlayer(ulong clientId, Team team) {
        SpawnPlayerServerRPC(clientId, team);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRPC(ulong clientId, Team team) {
        GameObject[] spawnPoints;
        GameObject spawnPrefab;

        if (team == Team.RUNNER) {
            spawnPoints = GameObject.FindGameObjectsWithTag(runnerSpawnTag);
            spawnPrefab = runnerPrefab;
        } else {
            spawnPoints = GameObject.FindGameObjectsWithTag(sharkSpawnTag);
            spawnPrefab = sharkPrefab;
        }
        GameObject spawnedPlayer = Instantiate(spawnPrefab, spawnPoints[0].transform);
        spawnedPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}
