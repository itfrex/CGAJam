using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BeamShot : ISpellBehaviour
{
    private const float SPEED = 5;
    Vector3 dir;
    public void Cast(PlayerController  player, Projectile proj){
        proj.rb.useGravity = false;
        dir = proj.transform.TransformDirection(Vector3.forward);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        RaycastHit hit;
        if(Physics.Raycast(proj.transform.position, dir, out hit, SPEED, proj.collisionLayers)){
            proj.transform.position = hit.point;
        }
        else{
            proj.transform.position = proj.transform.position + dir * SPEED;
        }
        proj.life -= 5;
   }
   public void End(PlayerController  player, Projectile proj){
        proj.transform.forward = dir;
        proj.rb.velocity = proj.transform.forward*SPEED;
   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
        proj.transform.forward = proj.rb.velocity;
        dir = proj.transform.forward;
   }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
        proj.transform.forward = dir;
        proj.rb.velocity = proj.transform.forward*SPEED/Time.fixedDeltaTime;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life = 0;
    }
}
