using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Runner validRunner = other.GetComponent<Runner>();
        if (validRunner && validRunner.hasKey.Value) {
            GameManager.Instance.CompleteGame(Team.RUNNER);
        }
    }
}
