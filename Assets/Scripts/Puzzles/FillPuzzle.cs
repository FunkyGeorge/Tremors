using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillPuzzle : MonoBehaviour
{
    [SerializeField] private Transform fillBar;
    [SerializeField] private Transform wholeFillMeter;
    [SerializeField] private Sprite unluckySprite;
    [SerializeField] private Sprite luckySprite;
    [SerializeField] private float timeToFill = 5f;

    private int playersStanding = 0;
    private float fullHeight;
    private float currentTime = 0;

    private PuzzleState state = PuzzleState.Waiting;

    void Start() {
        fullHeight = fillBar.localScale.y;
        fillBar.localScale = new Vector3(fillBar.localScale.x, 0, fillBar.localScale.z);
    }
    
    void Update() {
        if (state != PuzzleState.Solved && playersStanding > 0) {
            currentTime += Time.deltaTime;
            wholeFillMeter.gameObject.SetActive(true);

            // Set fillbar
            fillBar.localScale = new Vector3(fillBar.localScale.x, currentTime / timeToFill * fullHeight, fillBar.localScale.z);

            if (currentTime >= timeToFill) {
                SetSolved();
            }
        } else {
            wholeFillMeter.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Runner validRunner = collider.gameObject.GetComponent<Runner>();
        if (validRunner) {
            playersStanding++;
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        Runner validRunner = collider.gameObject.GetComponent<Runner>();
        if (validRunner) {
            playersStanding--;
        }
    }

    private void SetSolved() {
        state = PuzzleState.Solved;
        GetComponent<SpriteRenderer>().sprite = unluckySprite;
        // if (IsServer) {
        //     GameManager.Instance.CheckCompletePuzzle(serial.Value);
        // }
    }
}
