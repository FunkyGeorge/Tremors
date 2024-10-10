using System.Collections.Generic;
using UnityEngine;

public class PentaPuzzle : MonoBehaviour
{

    enum PuzzleState {
        Waiting,
        Solving,
        Solved
    }
    [SerializeField] private Sprite unluckySprite;
    [SerializeField] private Sprite luckySprite;
    [SerializeField] private List<PuzzleNode> nodes = new List<PuzzleNode>();
    private PuzzleState state = PuzzleState.Waiting;
    public int serial = -1;
    // Start is called before the first frame update
    void Start()
    {
        serial = GameManager.Instance.RegisterPuzzle(gameObject);
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].SetIndex(i);
            nodes[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (state == PuzzleState.Waiting) {
            Runner validRunner = collider.gameObject.GetComponent<Runner>();
            if (validRunner) {
                foreach (PuzzleNode node in nodes) {
                    node.gameObject.SetActive(true);
                    state = PuzzleState.Solving;
                }
            }
        }
    }

    public void OnToggleNode(int nodeIndex) {
        bool isSolved = true;
        for (int i = 0; i < nodes.Count; i++) {
            if (i == (0 + nodeIndex) % nodes.Count || i == (2 + nodeIndex) % nodes.Count || i == (3 + nodeIndex) % nodes.Count) {
                nodes[i].Toggle();
            }
            isSolved = isSolved && nodes[i].active;
        }

        if (isSolved) {
            SetSolved();
        }
    }

    private void SetSolved() {
        state = PuzzleState.Solved;
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        GetComponent<SpriteRenderer>().sprite = unluckySprite;
        GameManager.Instance.CheckCompletePuzzle(serial);
    }

    public void SetComplete() {
        state = PuzzleState.Solved;
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].gameObject.SetActive(false);
        }
        GetComponent<SpriteRenderer>().sprite = luckySprite;
    }
}
