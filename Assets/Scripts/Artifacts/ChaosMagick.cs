using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosMagick : IArtifactEffect
{
    public void ApplyEffect(float amt){
        GameController.Instance.GetPlayer().randomizeSpellOrder=true;
    }
}
