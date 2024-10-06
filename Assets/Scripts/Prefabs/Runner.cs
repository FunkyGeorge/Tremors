using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class Runner : NetworkBehaviour
{
    public NetworkVariable<bool> hasKey = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isFlipped = new NetworkVariable<bool>(false);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private Vector2 intentDirection = Vector2.zero;
    private const string V_CAM_NAME = "Virtual Camera";

    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject abilityUIPrefab;
    [SerializeField] private GameObject ghostPrefab;

    [Header("Movement")]
    [SerializeField] private float mSpeed = 100;
    [SerializeField] private float dodgeSpeed = 200;
    [SerializeField] private float dodgeTime = 0.5f;
    [SerializeField] private float dodgeCooldown = 5f;
    [SerializeField] private float keySpeedPenalty = 1f;
    private float dodgeCooldownTimer = 0;
    private float dodgeTimer = 0;
    private bool trackable = false;
    private bool onHighGround = false;

    [Header("Skill")]
    [SerializeField] private GameObject lurePrefab;
    [SerializeField] private float skillCooldown = 180f;
    private float skillCooldownTimer = 0;

    [Header("Sound")]
    [SerializeField] private AudioSource footstepSource;

    enum MoveState {
        Normal,
        Dodging
    }

    private MoveState currentState = MoveState.Normal;

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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
        sr.flipX = isFlipped.Value;

        if (!IsOwner) { return; }

        CheckCooldowns();
        CheckRadar();
        CheckMovement();
        Animate();
    }

    public override void OnDestroy()
    {
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        GameManager.Instance.CheckActiveRunners();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        onHighGround = true;
    }

    void OnTriggerExit2D(Collider2D collider) {
        onHighGround = false;
    }
    
    private void Move(Vector2 _move){
        float currentSpeed = mSpeed;
        if (hasKey.Value) {
            currentSpeed -= keySpeedPenalty;
        }
        rb.velocity = _move * currentSpeed;
    }

    private void Animate() {
        if (intentDirection.x > 0 && isFlipped.Value) {
            SetFlippedServerRPC(false);
        } else if (intentDirection.x < 0 && !isFlipped.Value) {
            SetFlippedServerRPC(true);
        }

        anim.SetBool("isDashing", currentState == MoveState.Dodging);
        anim.SetFloat("speed", intentDirection.magnitude);
    }

    private void CheckMovement() {
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

        bool shouldBeTrackable = intentDirection != Vector2.zero && !onHighGround;

        if (shouldBeTrackable != trackable) {
            GameManager.Instance.HandleTracked(gameObject);
            trackable = shouldBeTrackable;
        }
        
        if (intentDirection != Vector2.zero) {
            if (!footstepSource.isPlaying) {
                SetFootstepSoundClientRPC(true);
            }
        } else {
            SetFootstepSoundClientRPC(false);
        }
    }

    private void CheckCooldowns() {
        if (dodgeCooldownTimer >= 0) {
            dodgeCooldownTimer -= Time.deltaTime;
            UIManager.Instance.RefreshMovementOnWidget(dodgeCooldownTimer/dodgeCooldown);
        }

        if (skillCooldownTimer >= 0) {
            skillCooldownTimer -= Time.deltaTime;
            UIManager.Instance.RefreshUniqueOnWidget(skillCooldownTimer/skillCooldown);
        }
    }

    private void DodgeMove(Vector2 _move) {
        rb.velocity = _move * dodgeSpeed;

        dodgeTimer -= Time.deltaTime;

        if (dodgeTimer < 0) {
            currentState = MoveState.Normal;
        }
    }

    private void DropLure() {
        if (skillCooldownTimer <= 0) {
            SpawnLureServerRPC();
            skillCooldownTimer = skillCooldown;
        }
    }

    private void CheckRadar() {
        List<Vector2> radarInfo = GameManager.Instance.GetTremorPositions(transform.position);
        if (UIManager.Instance) {
            UIManager.Instance.RefreshRadar(transform.position, radarInfo, 13f);
        }
    }

    public void CollectKey() {
        CollectKeyClientRPC();
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

    [ClientRpc]
    private void CollectKeyClientRPC() {
        GameObject keySocket = transform.Find("Key Socket").gameObject;
        SpriteRenderer socketSprite = keySocket.GetComponent<SpriteRenderer>();
        socketSprite.sprite = keyPrefab.GetComponent<SpriteRenderer>().sprite;
        if (NetworkManager.Singleton.IsHost) {
            hasKey.Value = true;
        }
    }

    [ClientRpc]
    private void SetFootstepSoundClientRPC(bool active) {
        if (active) {
            footstepSource.Play();
        } else {
            footstepSource.Stop();
        }
    }

    [ServerRpc]
    private void SetFlippedServerRPC(bool flipped) {
        isFlipped.Value = flipped;
    }

    [ServerRpc(RequireOwnership = false)]
    void EliminateServerRPC(ulong clientId) {
        GetComponent<CapsuleCollider2D>().enabled = false;
        GameObject spectatorGhost = Instantiate(ghostPrefab, transform);
        spectatorGhost.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        Destroy(gameObject);
    }

    [ServerRpc]
    private void SpawnLureServerRPC() {
        GameObject lureObject = Instantiate(lurePrefab, transform.position, Quaternion.identity);
        lureObject.GetComponent<NetworkObject>().Spawn();
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

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.performed) {
            DropLure();
        }
    }
}
