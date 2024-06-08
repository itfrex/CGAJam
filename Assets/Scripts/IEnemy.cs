using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public bool Spawn();
    public bool Hit();
    public bool Destroy();
}
