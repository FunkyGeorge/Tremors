using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class Runner : NetworkBehaviour
{
    public NetworkVariable<bool> hasFlag = new NetworkVariable<bool>(false);

    private Vector2 intentDirection = Vector2.zero;
    private const string V_CAM_NAME = "Virtual Camera";

    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private GameObject abilityUIPrefab;
    [SerializeField] private GameObject ghostPrefab;

    [Header("Movement")]
    [SerializeField] private float mSpeed = 100;
    [SerializeField] private float dodgeSpeed = 200;
    [SerializeField] private float dodgeTime = 0.5f;
    [SerializeField] private float dodgeCooldown = 5f;
    private float dodgeCooldownTimer = 0;
    private float dodgeTimer = 0;

    enum MoveState {
        Normal,
        Dodging
    }

    private MoveState currentState = MoveState.Normal;

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;

        if (IsOwner) {
            // Set follow camera
            GameObject vCam = GameObject.Find(V_CAM_NAME);
            CinemachineVirtualCamera vCamComponent = vCam.GetComponent<CinemachineVirtualCamera>();
            vCamComponent.Follow = transform;

            // Create ability UI Widget
            UIManager.Instance.SetAbilityWidget(abilityUIPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        if (dodgeCooldownTimer >= 0) {
            dodgeCooldownTimer -= Time.deltaTime;
            UIManager.Instance.RefreshMovementOnWidget(dodgeCooldownTimer/dodgeCooldown);
        }

        CheckRadar();

        // Which kind of movement?
        switch(currentState) {
            default:
                Move(intentDirection);
                break;
            case MoveState.Normal:
                Move(intentDirection);
                break;
            case MoveState.Dodging:
                DodgeMove(intentDirection);
                break;
        }
    }

    public override void OnDestroy()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        GameManager.Instance.CheckActiveRunners();
    }
    
    private void Move(Vector2 _move){
        GetComponent<Rigidbody2D>().velocity = _move * mSpeed;
    }

    private void DodgeMove(Vector2 _move) {
        GetComponent<Rigidbody2D>().velocity = _move * dodgeSpeed;

        dodgeTimer -= Time.deltaTime;

        if (dodgeTimer < 0) {
            currentState = MoveState.Normal;
        }
    }

    private void CheckRadar() {
        List<Vector2> radarInfo = GameManager.Instance.GetTremorPositions(transform.position);
        if (UIManager.Instance) {
            UIManager.Instance.RefreshRadar(transform.position, radarInfo);
        }
    }

    public void CollectFlag() {
        CollectFlagClientRPC();
    }

    [ClientRpc]
    private void CollectFlagClientRPC() {
        GameObject flagSocket = transform.Find("Flag Socket").gameObject;
        SpriteRenderer socketSprite = flagSocket.GetComponent<SpriteRenderer>();
        socketSprite.sprite = flagPrefab.GetComponent<SpriteRenderer>().sprite;
        hasFlag.Value = true;
    }

    public void Eliminate() {
        EliminateServerRPC(OwnerClientId);
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
                    sr.color = LobbyAssets.GetCharacterColor(
                        Enum.Parse<PlayerColor>(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value));
                }
            });
    }

    [ServerRpc(RequireOwnership = false)]
    void EliminateServerRPC(ulong clientId) {
        GetComponent<CapsuleCollider2D>().enabled = false;
        GameObject spectatorGhost = Instantiate(ghostPrefab, transform);
        spectatorGhost.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        Destroy(gameObject);
    }

    // Player input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentState == MoveState.Normal) {
                intentDirection = context.ReadValue<Vector2>();
            }
        }

        if (context.canceled)
        {
            if (currentState == MoveState.Normal) {
                intentDirection = Vector2.zero;
            }
        }
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        if (context.performed) {
            if (dodgeCooldownTimer < 0) {
                dodgeCooldownTimer = dodgeCooldown;
                currentState = MoveState.Dodging;
                dodgeTimer = dodgeTime;
            }
        }
    }
}
