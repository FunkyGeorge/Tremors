using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Runner validRunner = other.GetComponent<Runner>();
        if (validRunner && validRunner.hasFlag.Value) {
            GameManager.Instance.CompleteGame(Team.RUNNER);
        }
    }
}
