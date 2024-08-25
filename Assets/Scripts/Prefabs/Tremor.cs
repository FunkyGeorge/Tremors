using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tremor : NetworkBehaviour
{
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

    enum MoveState {
        Normal,
        Coast,
        Charge,
    }

    private MoveState currentState = MoveState.Normal;

    // Start is called before the first frame update
    void Start()
    {
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
        if (IsOwner) {
            // What kind of movement?
            switch (currentState) {
                case MoveState.Charge:
                    currentSpeed = chargeSpeed;
                    Move(intentDirection, rotationSpeed * 2);
                    break;
                case MoveState.Coast:
                    currentSpeed = normalSpeed;
                    if (chargeCooldownTimer > 0) {
                        chargeCooldownTimer -= Time.deltaTime;
                        UIManager.Instance.RefreshMovementOnWidget(chargeCooldownTimer/chargeCooldown);
                    }
                    Coast(intentDirection);
                    break;
                case MoveState.Normal:
                    currentSpeed = normalSpeed;
                    if (chargeCooldownTimer > 0) {
                        chargeCooldownTimer -= Time.deltaTime;
                        UIManager.Instance.RefreshMovementOnWidget(chargeCooldownTimer/chargeCooldown);
                    }
                    Move(intentDirection, rotationSpeed);
                    break;
                default:
                    Move(intentDirection, rotationSpeed);
                    break;
            }

            if (scanCooldownTimer > 0) {
                scanCooldownTimer -= Time.deltaTime;
                UIManager.Instance.RefreshUniqueOnWidget(scanCooldownTimer/scanCooldown);
            }

            if (scanTimer > 0) {
                RunnerScan();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<Runner>().Eliminate();
        }
    }



    private void Move(Vector2 _move, float _rotationSpeed) {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (_move.magnitude > 0) {
            float targetAngle = -Mathf.Atan2(_move.x, _move.y) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref turnSmoothingMemo, _rotationSpeed/1000);
            rb.MoveRotation(angle);

            rb.velocity = rb.transform.up * currentSpeed;
        }
    }

    private void Coast(Vector2 _move) {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = rb.transform.up * normalSpeed;
    }

    private void StartRunnerScan() {
        scanTimer = scanDuration;
    }

    private void RunnerScan() {
        scanTimer -= Time.deltaTime;
        List<Vector2> runnerPositions = GameManager.Instance.GetRunnerPositions();
        UIManager.Instance.RefreshRadar(transform.position, runnerPositions, scanTimer);
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
            if (scanCooldownTimer <= 0) {
                scanCooldownTimer = scanCooldown;
                StartRunnerScan();
            }
        }

        if (context.started) {
        }

        if (context.canceled) {

        }
    }
}
