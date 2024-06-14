using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRateMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().firerateMult *= amt;
    }
}
