using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GlobalConfigSO gConfig;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private List<GameObject> radarIcons;
    [SerializeField] private Color radarColor;
    [SerializeField] private GameObject abilityWidgetSocket;
    private AbilityWidget abilityWidget;

    private static UIManager _instance;
    public static UIManager Instance{
        get {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gConfig.hideUI) {
            GetComponent<Canvas>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAbilityWidget(GameObject widgetPrefab) {
        GameObject widgetObject = Instantiate(widgetPrefab, abilityWidgetSocket.transform, false);
        abilityWidget = widgetObject.GetComponent<AbilityWidget>();
    }

    public void RefreshRadar(Vector3 playerPosition, List<Vector2> trackedPositions) {
        for (int i = 0; i < radarIcons.Count; i++) {
            if (i < trackedPositions.Count) {
                radarIcons[i].SetActive(true);
                Quaternion newAngle = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, trackedPositions[i] - new Vector2(playerPosition.x, playerPosition.y)));
                radarIcons[i].GetComponent<RectTransform>().rotation = newAngle;
                
                // Set Color intensity for radar icon
                Image radarImage = radarIcons[i].transform.GetComponentInChildren<Image>();
                Color fadedColor = radarColor;
                Color vibrantColor = radarColor;
                vibrantColor.a = 1f;
                fadedColor.a = 0.3f;
                float lerpAlpha = 1 - Vector2.Distance(trackedPositions[i], playerPosition) / 13f;
                radarImage.color = Color.Lerp(fadedColor, vibrantColor, lerpAlpha);
            } else {
                radarIcons[i].SetActive(false);
            }
        }
    }

    public void RefreshRadar(Vector3 playerPosition, List<Vector2> trackedPositions, float lerpAlpha) {
        for (int i = 0; i < radarIcons.Count; i++) {
            if (i < trackedPositions.Count) {
                radarIcons[i].SetActive(true);
                Quaternion newAngle = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, trackedPositions[i] - new Vector2(playerPosition.x, playerPosition.y)));
                radarIcons[i].GetComponent<RectTransform>().rotation = newAngle;
                
                // Set Color intensity for radar icon
                Image radarImage = radarIcons[i].transform.GetComponentInChildren<Image>();
                Color fadedColor = radarColor;
                Color vibrantColor = radarColor;
                vibrantColor.a = 1f;
                fadedColor.a = 0;
                radarImage.color = Color.Lerp(fadedColor, vibrantColor, lerpAlpha);
            } else {
                radarIcons[i].SetActive(false);
            }
        }
    }

    public void RefreshMovementOnWidget(float fill) {
        if (abilityWidget) {
            abilityWidget.RefreshMovementAbilityUI(fill);
        }
    }

    public void RefreshUniqueOnWidget(float fill) {
        if (abilityWidget) {
            abilityWidget.RefreshUniqueAbilityUI(fill);
        }
    }

    public void RefreshGameTimer(float timeLeft) {
        timerText.text = TimeSpan.FromSeconds(timeLeft).ToString("mm':'ss");
    }
}
