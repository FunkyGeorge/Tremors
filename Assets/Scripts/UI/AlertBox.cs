using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertBox : MonoBehaviour
{
    [SerializeField] private GameObject messageWidget;
    
    public void AddAlert(string alertText) {
        GameObject message = Instantiate(messageWidget);
        message.GetComponent<TMP_Text>().text = alertText;
        message.transform.SetParent(transform);
        StartCoroutine(DelayedMessageCleanup(message));
    }

    IEnumerator DelayedMessageCleanup(GameObject messageToDestroy) {
        yield return new WaitForSeconds(3);
        Destroy(messageToDestroy);
    }
}
