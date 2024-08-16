using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> radarIcons;
    [SerializeField] private Color radarColor;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
