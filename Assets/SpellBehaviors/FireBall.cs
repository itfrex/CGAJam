using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireBall : ISpellBehaviour
{
    public void Cast(PlayerController  player, Projectile proj){
        proj.rb.useGravity = false;
        proj.rb.velocity = proj.transform.TransformDirection(Vector3.forward);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        proj.transform.forward = proj.rb.velocity;
        proj.rb.velocity = proj.transform.forward * 10;
        proj.life -= 1;
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
   }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life = 0;
    }
}
