using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpellBehaviour
{
   public void Cast(PlayerController  player, Projectile proj);
   public void Sustain(PlayerController  player, Projectile proj);
   public void End(PlayerController  player, Projectile proj);
   public void SwitchTo(Projectile proj);
   public void SwitchOff(Projectile proj);
   public void Hit(PlayerController  player, Projectile proj, Collision col);
}
