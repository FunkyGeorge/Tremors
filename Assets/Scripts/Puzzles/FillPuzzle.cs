using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FillPuzzle : Puzzle
{
    [Header("Fill Puzzle Config")]
    [SerializeField] private Transform fillBar;
    [SerializeField] private Transform wholeFillMeter;
    [SerializeField] private float timeToFill = 5f;

    private NetworkVariable<int> playersStanding = new NetworkVariable<int>(0);
    private float fullHeight = 0;
    private float currentTime = 0;
    public NetworkVariable<float> fillHeight = new NetworkVariable<float>(0);

    protected override void InitializePuzzle() {
        
    }

    void Start() {
        fullHeight = fillBar.localScale.y;
        fillBar.localScale = new Vector3(fillBar.localScale.x, fillHeight.Value, fillBar.localScale.z);
    }
    
    void Update() {
        SyncFillbar();
        if (!IsServer) { return; }
        if (state != PuzzleState.Solved && playersStanding.Value > 0) {
            IncreaseCurrentTimeServerRPC();
        }
    }

    [ServerRpc]
    private void IncreaseCurrentTimeServerRPC() {
        currentTime += Time.deltaTime;
        fillHeight.Value = currentTime / timeToFill * fullHeight;

        if (currentTime >= timeToFill) {
            SetSolvedClientRPC();        
        }
    }

    private void SyncFillbar() {
        if (state != PuzzleState.Solved && playersStanding.Value > 0) {
            fillBar.localScale = new Vector3(fillBar.localScale.x, fillHeight.Value, fillBar.localScale.z);
            wholeFillMeter.gameObject.SetActive(true);
        } else {
            wholeFillMeter.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void SetSolvedClientRPC() {
        SetSolved();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!IsServer) { return; }
        Runner validRunner = collider.gameObject.GetComponent<Runner>();
        if (validRunner) {
            playersStanding.Value++;
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (!IsServer) { return; }
        Runner validRunner = collider.gameObject.GetComponent<Runner>();
        if (validRunner) {
            playersStanding.Value--;
        }
    }
}
