using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ghost : NetworkBehaviour
{
    private Vector2 intentDirection = Vector2.zero;
    private const string V_CAM_NAME = "Virtual Camera";
    private const string MAIN_CAM_NAME = "Main Camera";


    [Header("Movement")]
    [SerializeField] private float mSpeed = 100;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            // Set follow camera
            GameObject vCam = GameObject.Find(V_CAM_NAME);
            CinemachineVirtualCamera vCamComponent = vCam.GetComponent<CinemachineVirtualCamera>();
            vCamComponent.Follow = transform;

            GameObject mainCamera = GameObject.Find(MAIN_CAM_NAME);
            mainCamera.GetComponent<Camera>().cullingMask = -1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        Move(intentDirection);
    }

    public override void OnDestroy()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
    }
    
    private void Move(Vector2 _move){
        GetComponent<Rigidbody2D>().velocity = _move * mSpeed;
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        SyncColorClientRPC();
    }

    [ClientRpc]
    public void SyncColorClientRPC() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Lobby joinedLobby = LobbyManager.Instance.GetJoinedLobby();

        joinedLobby.Players.ForEach((player) => {
                if (ulong.Parse(player.Data[LobbyManager.KEY_CLIENT_ID].Value) == OwnerClientId) {
                    Color ghostColor = LobbyAssets.GetCharacterColor(
                        Enum.Parse<PlayerColor>(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value));
                    ghostColor.a = 0.7f;
                    sr.color = ghostColor;
                }
            });
    }


    // Player input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            intentDirection = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            intentDirection = Vector2.zero;
        }
    }
}
