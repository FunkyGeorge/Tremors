using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter2D(Collider2D other) {
        Runner validRunner = other.GetComponent<Runner>();
        if (validRunner) {
            validRunner.CollectFlag();
            Destroy(gameObject);
        }
    }
}