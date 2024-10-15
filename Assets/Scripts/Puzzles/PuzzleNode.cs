using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleNode : MonoBehaviour
{
    public bool active = false;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    private PentaPuzzle parentPuzzle;
    private int index = -1;
    // Start is called before the first frame update
    void Start()
    {
        parentPuzzle = transform.parent.GetComponent<PentaPuzzle>();
        if (!parentPuzzle) {
            Debug.LogError("Could not find parent puzzle!");
        }
        SyncColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIndex(int i) {
        index = i;
    }

    public void Toggle() {
        active = !active;
        SyncColor();
    }

    private void SyncColor() {
        if (active) {
            GetComponent<SpriteRenderer>().color = activeColor;
        } else {
            GetComponent<SpriteRenderer>().color = inactiveColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
            Runner validRunner = collider.gameObject.GetComponent<Runner>();
            if (validRunner) {
                parentPuzzle.OnToggleNode(index);
            }
    }
}
