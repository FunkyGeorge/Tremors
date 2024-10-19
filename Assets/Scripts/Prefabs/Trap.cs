using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Trap : NetworkBehaviour
{
    [SerializeField] private float activeTime = 5f;
    private bool isTriggered = false;
    private List<Runner> trappedRunners = new List<Runner>();
    private void OnTriggerEnter2D(Collider2D other) {
        Runner validRunner = other.GetComponent<Runner>();
        if (validRunner) {
            validRunner.ApplyTrap(transform.position);
            trappedRunners.Add(validRunner);
            if (!isTriggered) {
                isTriggered = true;
                // Currently in a layer only visible to tremors, set it to be visible by all
                gameObject.layer = 0;
                Invoke("End", activeTime);
            }
        }
    }

    private void End() {
        foreach (Runner runner in trappedRunners) {
            runner.Untrap();
        }
        if (IsOwner) {
            DestructServerRPC();
        }
    }

    [ServerRpc]
    private void DestructServerRPC() {
        Destroy(gameObject);
    }
}
