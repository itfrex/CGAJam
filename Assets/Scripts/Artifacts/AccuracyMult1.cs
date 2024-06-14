using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().accuracyMult *= amt;
    }
}
