using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tremor : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Vector2 intentDirection = Vector2.zero;
    private const string V_CAM_NAME = "Virtual Camera";

    [SerializeField] private GameObject abilityUIPrefab;

    [Header("Ability")]
    [SerializeField] private float scanCooldown = 10f;
    [SerializeField] private float scanDuration = 5f;
    [SerializeField] private float chargeCooldown = 3f;
    private float scanCooldownTimer = 0;
    private float scanTimer = 0;
    private float chargeCooldownTimer = 0;

    [Header("Movement")]
    [SerializeField] private float chargeSpeed = 100f;
    [SerializeField] private float normalSpeed = 50f;
    [SerializeField] private float rotationSpeed = 10f;
    private float currentSpeed = 1f;
    private float turnSmoothingMemo = 0;
    private float stunnedTimer = 0;
    private Vector2 lureTarget;

    [Header("Sound")]
    [SerializeField] private AudioSource chompSource;
    [SerializeField] private AudioSource slitherSource;

    enum MoveState {
        Normal,
        Coast,
        Charge,
        Lured,
        Stunned,
    }

    private MoveState currentState = MoveState.Normal;

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner) {
            rb = GetComponent<Rigidbody2D>();
            // Set follow camera
            GameObject vCam = GameObject.Find(V_CAM_NAME);
            CinemachineVirtualCamera vCamComponent = vCam.GetComponent<CinemachineVirtualCamera>();
            vCamComponent.Follow = transform;
            vCamComponent.m_Lens.OrthographicSize = 7.5f;


            // Create ability UI Widget
            UIManager.Instance.SetAbilityWidget(abilityUIPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner) {
            CheckMovement();
            CheckSlitherVolume();
            CheckCooldowns();
            RunnerScan();
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<Runner>().Eliminate();
            PlayChompSoundClientRPC();
        } else {
            currentState = MoveState.Stunned;
            stunnedTimer = 0.3f;
        }
    }

    private void CheckMovement() {
        switch (currentState) {
                case MoveState.Charge:
                    currentSpeed = chargeSpeed;
                    RotateBody(intentDirection, rotationSpeed * 2);
                    Move(intentDirection);
                    break;
                case MoveState.Coast:
                    currentSpeed = normalSpeed;
                    if (chargeCooldownTimer > 0) {
                        chargeCooldownTimer -= Time.deltaTime;
                        UIManager.Instance.RefreshMovementOnWidget(chargeCooldownTimer/chargeCooldown);
                    }
                    Coast();
                    break;
                case MoveState.Normal:
                    currentSpeed = normalSpeed;
                    if (chargeCooldownTimer > 0) {
                        chargeCooldownTimer -= Time.deltaTime;
                        UIManager.Instance.RefreshMovementOnWidget(chargeCooldownTimer/chargeCooldown);
                    }
                    RotateBody(intentDirection, rotationSpeed * 2);
                    Move(intentDirection);
                    break;
                case MoveState.Lured:
                    Vector2 luredVector = lureTarget - new Vector2(transform.position.x, transform.position.y);
                    luredVector.Normalize();
                    RotateBody(luredVector, rotationSpeed * 0.5f);
                    Move(luredVector);

                    if (Vector2.Distance(lureTarget, transform.position) < 0.1) {
                        currentState = MoveState.Normal;
                    }
                    break;
                case MoveState.Stunned:
                    if (stunnedTimer <= 0) {
                        currentState = MoveState.Normal;
                    }
                    stunnedTimer -= Time.deltaTime;
                    RotateBody(intentDirection, rotationSpeed * 2);
                    break;
                default:
                    RotateBody(intentDirection, rotationSpeed * 2);
                    Move(intentDirection);
                    break;
            }
    }

    private void CheckCooldowns() {
        if (scanCooldownTimer > 0) {
            scanCooldownTimer -= Time.deltaTime;
            UIManager.Instance.RefreshUniqueOnWidget(scanCooldownTimer/scanCooldown);
        }
    }

    private void CheckSlitherVolume() {
        switch (currentState) {
            case MoveState.Charge:
                SetSlitherVolumeClientRPC(0.15f);
                break;
            case MoveState.Normal:
            case MoveState.Coast:
                SetSlitherVolumeClientRPC(0.06f);
                break;
            default:
                SetSlitherVolumeClientRPC(0.06f);
                break;
        }
    }

    private void RotateBody(Vector2 _move, float _rotationSpeed) {
        if (_move.magnitude > 0) {
            float targetAngle = -Mathf.Atan2(_move.x, _move.y) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref turnSmoothingMemo, _rotationSpeed/1000);
            rb.MoveRotation(angle);
        }
    }

    private void Move(Vector2 _move) {
        if (_move.magnitude > 0) {
            rb.velocity = rb.transform.up * currentSpeed;
        }
    }

    private void Coast() {
        rb.velocity = rb.transform.up * normalSpeed;
    }

    private void RunnerScan() {
        List<Vector2> runnerPositions = GameManager.Instance.GetRunnerPositions();
        // Filter out positions too close
        float minRadarDistance = 8f;
        List<Vector2> filteredPositions = new List<Vector2>();
        foreach (Vector2 runnerPos in runnerPositions) {
            if (Vector2.Distance(runnerPos, gameObject.transform.position) > minRadarDistance) {
                filteredPositions.Add(runnerPos);
            }
        }

        UIManager.Instance.RefreshRadar(transform.position, filteredPositions, 50f);
    }

    public void SetLure(Vector2 lure) {
        currentState = MoveState.Lured;
        lureTarget = lure;
    }

    [ClientRpc]
    private void SetSlitherVolumeClientRPC(float volume) {
        if (!slitherSource.isPlaying) {
            slitherSource.Play();
        }

        slitherSource.volume = volume;
    }

    [ClientRpc]
    private void PlayChompSoundClientRPC(){
        chompSource.PlayOneShot(chompSource.clip);
    }
    

    // Player input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) { return; }
        if (context.performed)
        {
            intentDirection = context.ReadValue<Vector2>();
            if (currentState == MoveState.Coast) {
                currentState = MoveState.Normal;
            }
        }

        if (context.canceled)
        {
            if (currentState != MoveState.Charge) {
                currentState = MoveState.Coast;
            }
        }
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) { return; }
        if (context.performed) {
            
        }

        if (context.started) {
            if (chargeCooldownTimer <= 0) {
                currentState = MoveState.Charge;
            }
        }

        if (context.canceled) {
            if (currentState == MoveState.Charge) {
                currentState = MoveState.Coast;
                chargeCooldownTimer = chargeCooldown;
            }
        }
    }

    public void OnSkill(InputAction.CallbackContext context) {
        if (!IsOwner) { return; }
        if (context.performed) {
            // Deprecated, moving scan to always be on and add a new ability here
            // if (scanCooldownTimer <= 0) {
            //     scanCooldownTimer = scanCooldown;
            //     StartRunnerScan();
            // }
        }

        if (context.started) {
        }

        if (context.canceled) {

        }
    }
}
