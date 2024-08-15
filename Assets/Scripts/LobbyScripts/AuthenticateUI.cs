using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {


    [SerializeField] private Button authenticateButton;


    private void Awake() {
        authenticateButton.onClick.AddListener(async () => {
            try {
                await LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
                Hide();
            } catch (AuthenticationException e) {
                Debug.Log(e);
            }
        });
    }

    void Start() {
        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        if (joinedLobby != null) {
            Hide();
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}