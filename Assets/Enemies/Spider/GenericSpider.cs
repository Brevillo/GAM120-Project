using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSpider : GenericEnemy {

    public override IWhippable.Type WhippableType => IWhippable.Type.Heavy;

    protected override IEnumerator Behaviour() {

        return null;
    }
}
