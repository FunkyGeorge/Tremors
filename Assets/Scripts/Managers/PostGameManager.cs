using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

public class PostGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text winningText;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.Shutdown();
        Invoke("ShowWinningText", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowWinningText() {
        try {
            winningText.text = string.Format("{0} win!", LobbyManager.Instance.gameWinners == Team.RUNNER ? "Runners" : "Tremors");
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }

        Invoke("ReturnToLobby", 5f);
    }

    void ReturnToLobby() {
        if (NetworkManager.Singleton != null) {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (LobbyManager.Instance.IsLobbyHost()) {
            try {
                LobbyManager.Instance.ClearLobby();
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        SceneManager.LoadScene("Lobby");
    }
}
