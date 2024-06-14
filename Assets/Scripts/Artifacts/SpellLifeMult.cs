using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLifeMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().bulletDurationMult *= amt;
    }
}
