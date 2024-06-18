
using UnityEngine;
using UnityEngine.AI;

public class HomingShot : ISpellBehaviour
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 0.5f;
    private const float HOVER_HEIGHT = 1f;
    private const float ROT_SPEED = 5f;
     int navMask;
    public void Cast(PlayerController  player, Projectile proj){
     proj.player.SetCooldown(0.6f);
        proj.rb.useGravity = false;
     proj.transform.forward = (proj.transform.forward+0.05f*Random.insideUnitSphere*proj.player.accuracyMult).normalized;
        proj.rb.velocity = proj.transform.forward*10;
        InitHoming(proj);
        proj.player.ApplyKickback(0.3f);
   }
   
   public void Sustain(PlayerController  player, Projectile proj){
     if(proj.target == null){
          proj.target = GameController.Instance.GetNearestEnemy(proj.transform.position);
     }
     if(proj.target != null){
          Vector3 targetPos;
          if(proj.searchCooldown > 0){
               proj.searchCooldown -= Time.fixedDeltaTime;
          }else{
               proj.searchCooldown = CalculatePath(proj.transform.position, proj.target.transform.position, proj.path);
          }
          if (proj.path.corners.Length > 2 && Physics.Linecast(proj.transform.position, proj.target.transform.position, proj.collisionLayers)){
            targetPos = proj.path.corners[1];
            targetPos.y += HOVER_HEIGHT;
          }else{
            targetPos = proj.target.transform.position;
          }
        Vector3 moveVec = (targetPos - proj.transform.position).normalized;
        
        Quaternion rot = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(moveVec, Vector3.up)*Quaternion.Inverse(proj.rb.rotation),1);
        proj.rb.AddTorque(new Vector3(rot.x, rot.y, rot.z)*ROT_SPEED);
        //proj.transform.forward = Vector3.RotateTowards(proj.transform.forward, moveVec, ROT_SPEED,0);
          Debug.DrawRay(proj.transform.position, moveVec*10, Color.red);
          Debug.DrawLine(proj.transform.position, proj.target.transform.position);
     }
     else Debug.Log("NO TARGET!");
     
          proj.rb.velocity *= 0.95f;
          proj.rb.angularVelocity *= 0.9f;
        proj.rb.AddForce(proj.transform.forward*25);
        //proj.rb.angularVelocity = Vector3.zero;
        RaycastHit hit;
        if(Physics.Raycast(proj.transform.position, Vector3.down, out hit, HOVER_HEIGHT, proj.collisionLayers)){
            Debug.DrawRay(proj.transform.position, Vector3.down*HOVER_HEIGHT, Color.red);
            proj.rb.AddForce(Vector3.up * 10 * (1 - hit.distance/HOVER_HEIGHT));
        }
        //proj.transform.forward = proj.rb.velocity;
        //proj.rb.velocity = proj.transform.forward * 10;
        proj.life -= 1;
   }
   public void End(PlayerController  player, Projectile proj){

   }
   public void SwitchTo(Projectile proj){
        proj.rb.useGravity = false;
        InitHoming(proj);
      }
   public void SwitchOff(Projectile proj){
        proj.rb.useGravity= false;
          proj.rb.angularVelocity = Vector3.zero;
        //if(proj.target == null){
          proj.transform.forward = proj.rb.velocity;
        //}
        //else{
          //proj.transform.forward = (proj.target.transform.position - proj.transform.position).normalized;
          //proj.rb.velocity = proj.transform.forward * proj.rb.velocity.magnitude;
        //}
   }
   public void Hit(PlayerController  player, Projectile proj, Collision col){
          if(col.collider.CompareTag("Enemy")){
            proj.life -= 100;
        }
        if(proj.target == null){
            proj.target = GameController.Instance.GetNearestEnemy(proj.transform.position);
        }
   }
    void InitHoming(Projectile proj){
          navMask = NavMesh.GetAreaFromName("Flying");
          proj.path = new NavMeshPath();
          proj.searchCooldown = 0;
    }

    float CalculatePath(Vector3 from, Vector3 to, NavMeshPath path){
            NavMeshHit hit; 
            if(NavMesh.SamplePosition(from, out hit, 100, navMask)){
               if(NavMesh.CalculatePath(hit.position, to, navMask, path)){
                    return 0.1f + Vector3.Distance(from,to)*WAIT_FACTOR;
               }
               else {    
               Debug.Log("Failed to find Path!");
               return SEARCH_DELAY;
               }
            }
            else {
               Debug.Log("Failed to Sample Position!");
               return SEARCH_DELAY;
            }
    }
}
