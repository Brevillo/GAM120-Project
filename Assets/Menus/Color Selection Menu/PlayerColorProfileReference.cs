using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorProfileReference : ScriptableObject {

    private PlayerColorProfile playerColorProfile;

    public PlayerColorProfile PlayerColorProfile {
        get => playerColorProfile;
        set => playerColorProfile = value;
    }
}
