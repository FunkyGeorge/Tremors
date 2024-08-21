using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityWidget : MonoBehaviour
{
    [SerializeField] private Image movementCooldownOverlay;
    [SerializeField] private Image uniqueCooldownOverlay;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshMovementAbilityUI(float fill) {
        movementCooldownOverlay.fillAmount = fill;
    }

    public void RefreshUniqueAbilityUI(float fill) {
        uniqueCooldownOverlay.fillAmount = fill;
    }
}
