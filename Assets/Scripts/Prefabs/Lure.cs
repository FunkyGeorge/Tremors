using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lure : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        Tremor validTremor = other.GetComponent<Tremor>();
        if (validTremor) {
            validTremor.SetLure(transform.position);
            Invoke("DestroySelf", 1f);
        }
    }

    private void DestroySelf() {
        Destroy(gameObject);
    }
}
