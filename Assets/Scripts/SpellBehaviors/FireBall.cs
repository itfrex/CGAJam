using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireBall : ISpellBehaviour
{
    public void Cast(PlayerController  player, Projectile proj){
     proj.player.SetCooldown(0.3f);
        proj.rb.useGravity = false;
        proj.transform.forward = (proj.transform.forward+0.025f*Random.insideUnitSphere*proj.player.accuracyMult).normalized;
        proj.rb.velocity = proj.transform.TransformDirection(Vector3.forward);
        proj.player.ApplyKickback(0.5f);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        proj.transform.forward = proj.rb.velocity;
        proj.rb.velocity = proj.transform.forward * 15;
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
    proj.life -= 100;
    }
}
