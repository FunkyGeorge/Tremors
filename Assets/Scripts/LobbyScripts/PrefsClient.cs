using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PrefsClient
{
    private const string CURRENT_LOBBY_KEY = "CURRENT_LOBBY_KEY";

    public static void SetPlayerLobby(string lobbyKey) {
        PlayerPrefs.SetString(CURRENT_LOBBY_KEY, lobbyKey);
    }

    public static void ClearPlayerLobby() {
        PlayerPrefs.DeleteKey(CURRENT_LOBBY_KEY);
    }

    public static string GetPlayerLobby() {
        return PlayerPrefs.GetString(CURRENT_LOBBY_KEY);
    }
}
