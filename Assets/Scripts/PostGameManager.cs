using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using System;

public class PostGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text winningText;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("ShowWinningText", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowWinningText() {
        try {
            Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();

            Team winners = Enum.Parse<Team>(joinedLobby.Data[LobbyManager.KEY_WINNING_TEAM].Value);

            winningText.text = String.Format("{0} win!", winners == Team.RUNNER ? "Runners" : "Tremors");
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
}
