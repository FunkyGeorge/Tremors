using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private GameObject uiSpritePrefab;
    [SerializeField] private GameObject uiSpriteContainer;
    private int survivorCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnSurvivorsUpdated += OnUpdateSurvivorsDashboard;

        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        if (joinedLobby != null) {
            int tremors = joinedLobby.Data[LobbyManager.KEY_TREMOR_IDS].Value.Split("_").Length;
            survivorCount = joinedLobby.Players.Count - tremors;
        } else {
            survivorCount = 1;
        }
        RefreshScoreboard();
    }

    void OnDestroy() {
        GameManager.Instance.OnSurvivorsUpdated -= OnUpdateSurvivorsDashboard;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnUpdateSurvivorsDashboard(object sender, int remaining) {
        if (remaining != survivorCount) {
            survivorCount = remaining;
            RefreshScoreboard();
        }
    }

    void RefreshScoreboard() {
        RectTransform containerRect = uiSpriteContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(100 * survivorCount, containerRect.sizeDelta.y);

        // There should always be 8 sprite children
        for (int i = 0; i < 8; i++) {
            uiSpriteContainer.transform.GetChild(i).gameObject.SetActive(i + 1 <= survivorCount);
        }
    }
}
