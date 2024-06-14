using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FloatingSkull : MonoBehaviour, IEnemy
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 1;
    private const float HOVER_HEIGHT = 1f;
    private const float ROT_SPEED = 0.75f;
    private const float SPEED = 10f;
    private Rigidbody rb;
    private int id;
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private AudioClip[] livingSounds;
    private NavMeshPath path;
    private int areaMask;
    
    private float wobble;
    private Vector3 moveVec;
    private Vector3 targetPos;
    private Coroutine searchRoutine;
    [SerializeField] private LayerMask collisionLayers;
    public bool Spawn(int id){
        this.id = id;
        return true;
    }
    public bool Hit(){
        return Kill();  
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
        audioSource = GetComponent<AudioSource>();
        areaMask = NavMesh.GetAreaFromName("Flying");
        path = new NavMeshPath();
        searchRoutine = StartCoroutine(CalculatePath(GameController.Instance.GetPlayer().gameObject));
    }
    void FixedUpdate() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, HOVER_HEIGHT, collisionLayers)){
            Debug.DrawRay(transform.position, Vector3.down*HOVER_HEIGHT, Color.red);
            rb.AddForce(Vector3.up * 10 * (1 - hit.distance/HOVER_HEIGHT));
        }
        rb.AddForce(Vector3.up * Mathf.Cos(wobble)*0.5f);
        if (path.corners.Length > 2 && Physics.Linecast(transform.position, GameController.Instance.GetPlayer().transform.position, collisionLayers)){
            targetPos = path.corners[1];
            targetPos.y += HOVER_HEIGHT;
        }else{
            targetPos = GameController.Instance.GetPlayer().transform.position;
        }
        moveVec = (targetPos - transform.position).normalized;
        wobble += Time.fixedDeltaTime;
        
        Quaternion rot = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(moveVec, Vector3.up)*Quaternion.Inverse(rb.rotation),1);
        rb.AddTorque(new Vector3(rot.x, rot.y, rot.z)*ROT_SPEED);
        rb.AddForce(transform.forward*SPEED/(1+Vector3.Angle(transform.forward,moveVec)*0.2f));
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
}
