using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Bouncer : MonoBehaviour, IEnemy
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 1;
    private const float SPEED = 10f;
    private const float JUMP_COOLDOWN = 0.25f;
    private const float LUNGE_DIST = 10f;
    private Rigidbody rb;
    private int id; 
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private AudioClip[] livingSounds;
    private NavMeshPath path;
    private int areaMask;
    private int health = 2;
    private float jump_timer;
    private Vector3 targetPos;
    private Vector3 targetVel;
    private Coroutine searchRoutine;
    [SerializeField] private LayerMask collisionLayers;
    public bool Spawn(int id){
        this.id = id;
        return true;
    }
    public bool Hit(){
        if(health > 0){
            health -= 1;
        }
        else{
            Kill();  
        }
        return true;
    }
    public bool Kill(){
        AudioSource.PlayClipAtPoint(deathSounds[Random.Range(0, deathSounds.Length)], transform.position);
        Destroy(Instantiate(deathParticles, transform.position, transform.rotation), 5);
        StopCoroutine(searchRoutine);
        GameController.Instance.RemoveEnemy(id);
        Destroy(gameObject);
        return true;
    }
    public void SetId(int id){
        this.id = id;
    }
    void Start() {
        rb = GetComponent<Rigidbody>();
        areaMask = NavMesh.GetAreaFromName("Flying");
        path = new NavMeshPath();
        jump_timer = JUMP_COOLDOWN;
        searchRoutine = StartCoroutine(CalculatePath(GameController.Instance.GetPlayer().gameObject));
    }
    void FixedUpdate() {
        if (rb.velocity.magnitude > 0.5f){
                jump_timer = JUMP_COOLDOWN;
        }
        else{
            if(jump_timer > 0) {
                    jump_timer -= Time.fixedDeltaTime;
            }
            else{
                if (path.corners.Length > 2 && Physics.Linecast(transform.position, GameController.Instance.GetPlayer().transform.position, collisionLayers)){
                    targetPos = path.corners[1];
                    targetVel = Vector3.zero;
                    rb.velocity = FindLaunchAngle(transform.position, targetPos, targetVel);
                }else{
                    targetPos = GameController.Instance.GetPlayer().transform.position;
                    targetVel = GameController.Instance.GetPlayer().rb.velocity;
                    if(Vector3.Distance(targetPos, transform.position) < LUNGE_DIST){
                        rb.velocity = (Vector3.up*Vector3.Distance(targetPos, transform.position)/2 +
                        targetPos - transform.position + targetVel/2).normalized * SPEED;
                    }else{
                        rb.velocity = FindLaunchAngle(transform.position, targetPos, targetVel);
                    }
                }
                AudioSource.PlayClipAtPoint(livingSounds[Random.Range(0, livingSounds.Length)], transform.position);
            }
        }
    }
    void OnCollisionEnter(Collision col){
        if (col.gameObject.tag == "Player"){
            GameController.Instance.GetPlayer().Hurt();
        }
    }
    
    IEnumerator CalculatePath(GameObject obj){
        while(gameObject != null){
            NavMeshHit hit; 
            if(NavMesh.SamplePosition(transform.position, out hit, 10, areaMask)){
                if(NavMesh.CalculatePath(hit.position, obj.transform.position, areaMask, path)){
                    yield return new WaitForSeconds(0.1f + Vector3.Distance(obj.transform.position, transform.position)*WAIT_FACTOR);
                }
                else{
                    yield return new WaitForSeconds(SEARCH_DELAY);
                }
            }else{
                    yield return new WaitForSeconds(SEARCH_DELAY);
                }
        }
    }
    private Vector3 FindLaunchAngle(Vector3 from, Vector3 to, Vector3 targetVel){
        //prediction
        float horizDelta = Vector3.Distance(from-from.y*Vector3.up,to-to.y*Vector3.up);
        float t = Mathf.Min(horizDelta/SPEED,2);
        Vector3 targetOffset = targetVel * t;
        to += targetOffset;
        //jump
        horizDelta = Vector3.Distance(from-from.y*Vector3.up,to-to.y*Vector3.up);
        t = horizDelta/SPEED;
        float vertDelta = to.y-from.y;
        float yVel = vertDelta/t - Physics.gravity.y*t/2;
        return (to-to.y*Vector3.up-from-from.y*Vector3.up).normalized*SPEED + Vector3.up*yVel;
    }
}
