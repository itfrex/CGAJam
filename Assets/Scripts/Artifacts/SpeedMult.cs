using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().speed *= amt;
    }
}
