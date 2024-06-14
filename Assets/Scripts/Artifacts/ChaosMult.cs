using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosMult : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().chaosMult *= amt;
    }
}
