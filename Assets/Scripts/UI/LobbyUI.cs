using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour {


    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Transform container2;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button changeToBlueButton;
    [SerializeField] private Button changeToGreenButton;
    [SerializeField] private Button changeToLightGreenButton;
    [SerializeField] private Button changeToOrangeButton;
    [SerializeField] private Button changeToPinkButton;
    [SerializeField] private Button changeToPurpleButton;
    [SerializeField] private Button changeToRedButton;
    [SerializeField] private Button changeToTealButton;
    [SerializeField] private Button changeToWhiteButton;
    [SerializeField] private Button changeToYellowButton;


    private void Awake() {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        changeToBlueButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Blue);
            PrefsClient.SetPlayerColor(PlayerColor.Blue);
        });
        changeToGreenButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Green);
            PrefsClient.SetPlayerColor(PlayerColor.Green);
        });
        changeToLightGreenButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.LightGreen);
            PrefsClient.SetPlayerColor(PlayerColor.LightGreen);
        });
        changeToOrangeButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Orange);
            PrefsClient.SetPlayerColor(PlayerColor.Orange);
        });
        changeToPinkButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Pink);
            PrefsClient.SetPlayerColor(PlayerColor.Pink);
        });
        changeToPurpleButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Purple);
            PrefsClient.SetPlayerColor(PlayerColor.Purple);
        });
        changeToRedButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Red);
            PrefsClient.SetPlayerColor(PlayerColor.Red);
        });
        changeToTealButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Teal);
            PrefsClient.SetPlayerColor(PlayerColor.Teal);
        });
        changeToWhiteButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.White);
            PrefsClient.SetPlayerColor(PlayerColor.White);
        });
        changeToYellowButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerColor(PlayerColor.Yellow);
            PrefsClient.SetPlayerColor(PlayerColor.Yellow);
        });


        startGameButton.onClick.AddListener(() => {
            TMP_Text textBlock = startGameButton.transform.GetComponentInChildren<TMP_Text>();
            textBlock.text = "Starting...";
            LobbyManager.Instance.OnStartGame();
        });

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });
    }

    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
    }

    private void OnDestroy() {
        LobbyManager.Instance.OnJoinedLobby -= UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnLeftLobby;
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e) {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

        int playerIndex = 0;
        foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, playerIndex % 2 == 0 ? container : container2);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
            playerIndex++;
        }

        if (LobbyManager.Instance.HasStartedGame()) {
            SceneManager.LoadScene("Desert");
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        Show();
    }

    private void ClearLobby() {
        foreach (Transform child in container) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Transform child in container2) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
    }

}