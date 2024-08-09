using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tremor : NetworkBehaviour
{
    private Vector2 intentDirection = Vector2.zero;
    private const string V_CAM_NAME = "Virtual Camera";

    [Header("Movement")]
    [SerializeField] private float speed = 100f;
    [SerializeField] private float coastSpeed = 50f;
    [SerializeField] private float rotationSpeed = 10f;
    private float currentSpeed = 1f;
    private float turnSmoothingMemo = 0;

    enum MoveState {
        Normal,
        Coast,
    }

    private MoveState currentState = MoveState.Normal;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) {
            enabled = false;
            return;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Player localPlayer = LobbyManager.Instance.GetPlayer();
        Debug.Log("Logging Color: " + localPlayer.Data[LobbyManager.KEY_PLAYER_COLOR].Value);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = coastSpeed;

        if (IsOwner) {
            // Set follow camera
            GameObject vCam = GameObject.Find(V_CAM_NAME);
            CinemachineVirtualCamera vCamComponent = vCam.GetComponent<CinemachineVirtualCamera>();
            vCamComponent.Follow = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // What kind of movement?
        switch (currentState) {
            case MoveState.Coast:
                Coast(intentDirection);
                break;
            case MoveState.Normal:
            default:
                Move(intentDirection, rotationSpeed);
                break;
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
        rb.velocity = rb.transform.up * coastSpeed;
    }
    

    // Player input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            intentDirection = context.ReadValue<Vector2>();
            currentState = MoveState.Normal;
        }

        if (context.canceled)
        {
            currentState = MoveState.Coast;
        }
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        if (context.performed) {
            Debug.Log("Action");
        }

        if (context.started) {
            currentSpeed = speed;
        }

        if (context.canceled) {
            currentSpeed = coastSpeed;
        }
    }
}
