using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BeamShot : ISpellBehaviour
{
    private const float SPEED = 5;
    public void Cast(PlayerController  player, Projectile proj){
        proj.player.SetCooldown(0.8f);
        proj.rb.useGravity = false;
        proj.dir = proj.transform.TransformDirection(Vector3.forward);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        RaycastHit hit;
        if(Physics.Raycast(proj.transform.position, proj.dir, out hit, SPEED, proj.collisionLayers|proj.enemyLayers)){
            proj.transform.position = hit.point;
        }
        else{
            proj.transform.position = proj.transform.position + proj.dir * SPEED;
        }
        proj.life -= 5;
   }
   public void End(PlayerController  player, Projectile proj){
        proj.transform.forward = proj.dir;
        proj.rb.velocity = proj.transform.forward*SPEED;
   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
        //proj.transform.forward = proj.rb.velocity;
        proj.dir = proj.transform.forward;
   }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
        proj.transform.forward = proj.dir;
        proj.rb.velocity = proj.transform.forward*SPEED/Time.fixedDeltaTime;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life -= 80;
    }
}
