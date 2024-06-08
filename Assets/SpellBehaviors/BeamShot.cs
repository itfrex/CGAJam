using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEditor.Rendering;
using UnityEngine;

public class BeamShot : ISpellBehaviour
{
    private const float SPEED = 5;
    Vector3 dir;
    public void Cast(PlayerController  player, Projectile proj){
        proj.rb.useGravity = false;
        dir = proj.transform.TransformDirection(Vector3.forward);
        Debug.LogWarning(dir);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
        RaycastHit hit;
        if(Physics.Raycast(proj.transform.position, dir, out hit, SPEED, proj.collisionLayers)){
            Debug.Log("Hit!");
            proj.transform.position = hit.point;
        }
        else{
            proj.transform.position = proj.transform.position + dir * SPEED;
        }
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
        proj.transform.forward = proj.rb.velocity;
        dir = proj.transform.forward;
   }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
        proj.transform.forward = dir;
        proj.rb.velocity = proj.transform.forward*SPEED;
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
    proj.life = 0;
    if(col.gameObject.layer == 8){
        Debug.Log("Hit!");
    }
    }
}
