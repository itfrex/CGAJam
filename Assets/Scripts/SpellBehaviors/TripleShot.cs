using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TripleShot : ISpellBehaviour
{public void Cast(PlayerController  player, Projectile proj){
          proj.player.SetCooldown(1);
        proj.rb.velocity = proj.transform.TransformDirection(Vector3.forward*20);
        proj.rb.useGravity = true;
        Projectile[] copies = proj.player.DuplicateSpell(proj,2);
        proj.rb.velocity += proj.transform.TransformDirection(Random.insideUnitCircle*3*proj.player.accuracyMult);
        if(copies[0]!=null)copies[0].rb.velocity += proj.transform.TransformDirection(Random.insideUnitCircle*3*proj.player.accuracyMult);
        if(copies[1]!=null)copies[1].rb.velocity += proj.transform.TransformDirection(Random.insideUnitCircle*3*proj.player.accuracyMult);
        proj.player.ApplyKickback(2f);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        proj.transform.forward = proj.rb.velocity;
        proj.life -= 6f;
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = true;
        if(proj.isActiveAndEnabled){
          Projectile[] copies = proj.player.DuplicateSpell(proj,2);
        if(copies[0]!=null)copies[0].rb.velocity += proj.transform.TransformDirection(Random.insideUnitCircle*3*proj.player.accuracyMult);
        if(copies[1]!=null)copies[1].rb.velocity += proj.transform.TransformDirection(Random.insideUnitCircle*3*proj.player.accuracyMult);
        }
   }
   public void SwitchOff(Projectile proj){
     proj.rb.useGravity= false;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life -= 20 + proj.life/2;
   }
}
