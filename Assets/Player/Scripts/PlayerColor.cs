using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlayerColor : MonoBehaviour {

    [SerializeField] private PlayerColorProfile colorProfile;

    [SerializeField] private SpriteRenderer head, abdomen, thorax, wing, wingCasing;
    [SerializeField] private SpriteRenderer[] legs;

    private void UpdateColors() {

        if (colorProfile != null) {
            head        .color = colorProfile.HeadColor;
            abdomen     .color = colorProfile.AbdomenColor;
            thorax      .color = colorProfile.ThoraxColor;
            wing        .color = colorProfile.WingColor;
            wingCasing  .color = colorProfile.WingCasingColor;
            foreach (var leg in legs) leg.color = colorProfile.LegColor;
        }
    }

    private void Update() { 
        UpdateColors();
    }
}
