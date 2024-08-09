using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PrefsClient
{
    private const string CURRENT_LOBBY_KEY = "CURRENT_LOBBY_KEY";
    private const string USERNAME_KEY = "USERNAME";
    private const string PLAYER_COLOR_KEY = "PLAYER_COLOR";

    public static void SetPlayerLobby(string lobbyKey) {
        PlayerPrefs.SetString(CURRENT_LOBBY_KEY, lobbyKey);
    }

    public static void ClearPlayerLobby() {
        PlayerPrefs.DeleteKey(CURRENT_LOBBY_KEY);
    }

    public static string GetPlayerLobby() {
        return PlayerPrefs.GetString(CURRENT_LOBBY_KEY, "");
    }

    public static void SetUsername(string newUsername) {
        PlayerPrefs.SetString(USERNAME_KEY, newUsername);
    }

    public static string GetUsername(string defaultUsername) {
        return PlayerPrefs.GetString(USERNAME_KEY, defaultUsername);
    }

    public static void SetPlayerColor(PlayerColor newColor) {
        PlayerPrefs.SetString(PLAYER_COLOR_KEY, newColor.ToString());
    }

    public static PlayerColor GetPlayerColor() {
        return Enum.Parse<PlayerColor>(PlayerPrefs.GetString(PLAYER_COLOR_KEY, PlayerColor.White.ToString()));
    }
}
