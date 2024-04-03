using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectionButton : MonoBehaviour
{
    [SerializeField] private PlayerColor playerColorDisplayPrefab;
    [SerializeField] private Button button;

    public Button Initialize(PlayerColorProfile profile) {
        playerColorDisplayPrefab.SetProfile(profile);
        return button;
    }
}
