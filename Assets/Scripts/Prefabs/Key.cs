using UnityEngine;

public class Key : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        Runner validRunner = other.GetComponent<Runner>();
        if (validRunner) {
            validRunner.CollectKey();
        }
    }
}
