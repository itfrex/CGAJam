using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBall : ISpellBehaviour
{
   public void Cast(PlayerController  player, Projectile proj){
     proj.player.SetCooldown(0.4f);
     proj.transform.forward = (proj.transform.forward+0.03f*Random.insideUnitSphere*proj.player.accuracyMult).normalized;
        proj.rb.velocity = proj.transform.TransformDirection(Vector3.forward*15);
        proj.rb.useGravity = true;
        proj.player.ApplyKickback(0.7f);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        proj.transform.forward = proj.rb.velocity;
        proj.life -= 0.1f;
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = true;
   }
   public void SwitchOff(Projectile proj){
     proj.rb.useGravity= false;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life -= 5 + proj.life/2;
    if(col.gameObject.layer == 8){
        Debug.Log("Hit!");
    }
   }
}
