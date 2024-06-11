using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HelixShot : ISpellBehaviour
{
    public void Cast(PlayerController  player, Projectile proj){
     proj.player.SetCooldown(0.35f);
        proj.rb.useGravity = false;
        proj.rb.velocity = proj.transform.forward;
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        //proj.transform.forward = proj.rb.velocity;
        proj.rb.velocity = (proj.transform.forward + proj.transform.up) * 10;
        proj.rb.angularVelocity = proj.transform.forward*100;
        proj.life -= 1;
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
   }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
        proj.transform.forward = proj.rb.velocity;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life -= 50;
    if(col.gameObject.layer == 8){
        Debug.Log("Hit!");
    }
    }
}
