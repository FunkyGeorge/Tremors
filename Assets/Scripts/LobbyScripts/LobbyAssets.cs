using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }

    [SerializeField] private Sprite defaultPlayerSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite() {
        return defaultPlayerSprite;
    }

    public static Color GetCharacterColor(PlayerColor playerColor) {
        switch (playerColor) {
            case PlayerColor.Blue: return new Color(0, 0.4535954f, 1, 1);
            case PlayerColor.Green: return new Color(0, 0.5471698f, 0.03046552f, 1);
            case PlayerColor.LightGreen: return new Color(0.171875f, 1, 0, 1);
            case PlayerColor.Orange: return new Color(1, 0.4713461f, 0, 1);
            case PlayerColor.Pink: return new Color(1, 0.3820755f, 0.809046f, 1);
            case PlayerColor.Purple: return new Color(0.5666342f, 0, 1, 1);
            case PlayerColor.Red: return new Color(1, 0, 0.3146515f, 1);
            case PlayerColor.Teal: return new Color(0, 1, 0.8663819f, 1);
            case PlayerColor.White: return new Color(1, 1, 1, 1);
            case PlayerColor.Yellow: return new Color(1, 0.9423085f, 0, 1);
            default:
                return Color.white;
        }
    }

}