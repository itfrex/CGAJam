using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().knockbackMult *= amt;
    }
}
