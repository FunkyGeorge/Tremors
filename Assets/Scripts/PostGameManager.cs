using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PostGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text winningText;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("ShowWinningText", 1f);
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

        Invoke("ReturnToLobby", 5f);
    }

    async void ReturnToLobby() {
        try {
            Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
            await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { LobbyManager.KEY_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, "") },
                    { LobbyManager.KEY_TREMOR_IDS, new DataObject(DataObject.VisibilityOptions.Member, "") },
                    { LobbyManager.KEY_WINNING_TEAM, new DataObject(DataObject.VisibilityOptions.Member, "") }
                }
            });

            LobbyManager.Instance.AssignPlayerConnectionId(100);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }

        SceneManager.LoadScene("Lobby");
    }
}
