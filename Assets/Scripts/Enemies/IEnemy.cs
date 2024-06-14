using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public bool Spawn(int id);
    public bool Hit();
    public bool Kill();
    public void SetId(int id);
}
