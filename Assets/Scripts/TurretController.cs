using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TurretController : MonoBehaviour, IEnemy
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 1;
    private const float TARGET_TIME = 3;
    private const float SHOOT_COOLDOWN = 3;
    private const float ROT_SPEED = 0.075f;

    private int id;
    private NavMeshAgent agent;
    private NavMeshPath path;
    private Coroutine searchRoutine;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private GameObject deathParticles;
    private Vector3 aimDir;
    private Vector3 targetDir;
    private float timer;
    private LineRenderer line;
    [SerializeField] LayerMask collisionLayer;
    private int health = 3;
    [SerializeField] AnimationCurve targetingAnimCurve;
    [SerializeField] GameObject model;
    [SerializeField] AudioSource audioSource;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        searchRoutine = StartCoroutine(CalculatePath(GameController.Instance.GetPlayer().gameObject));
        line = GetComponent<LineRenderer>();
        aimDir = Random.onUnitSphere;
    }

    public bool Spawn(int id){
        this.id = id;
        health = 3;
        timer = SHOOT_COOLDOWN+TARGET_TIME;
        audioSource.PlayScheduled(AudioSettings.dspTime + SHOOT_COOLDOWN);
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
    public bool Hit(){
        health -= 1;
        if(health == 0){
            return Kill();
        }
        return true;
    }
    
    public void SetId(int id){
        this.id = id;
    }
    void FixedUpdate(){
        line.SetPosition(0, transform.position);
        targetDir = GameController.Instance.GetPlayer().transform.position-transform.position;
        Debug.DrawRay(transform.position, targetDir, Color.red);
        aimDir = Vector3.RotateTowards(aimDir, targetDir.normalized, ROT_SPEED/targetDir.magnitude, 0);
        Debug.DrawRay(transform.position, aimDir, Color.cyan);
        model.transform.forward = aimDir;
        if (timer > TARGET_TIME ){
            line.SetPosition(1, transform.position);
        }else{
            line.startWidth = targetingAnimCurve.Evaluate(timer/TARGET_TIME);
            RaycastHit hit;
            if(Physics.Raycast(transform.position, aimDir, out hit, 100f, collisionLayer)){
                line.SetPosition(1, hit.point);
                Debug.DrawLine(transform.position, hit.point);
            }else{
                line.SetPosition(1, transform.position + aimDir*100);
                Debug.DrawLine(transform.position, transform.position + aimDir*100);
            }
            if(timer <= 0){
                    if(Physics.Raycast(transform.position, aimDir, out hit, 100f, collisionLayer) && hit.transform.CompareTag("Player")){
                        GameController.Instance.GetPlayer().Hurt();
                    }
                    timer = SHOOT_COOLDOWN+TARGET_TIME;
                    audioSource.PlayScheduled(AudioSettings.dspTime + SHOOT_COOLDOWN);
                }
        }
        timer -= Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.tag == "Player"){
            GameController.Instance.GetPlayer().Hurt();
        }
    }
    IEnumerator CalculatePath(GameObject obj){
        while(gameObject != null){
            Vector3 targetPos;
            if(GameController.Instance.GetPlayerNavPoint(out targetPos)){
                if(agent.CalculatePath(targetPos, path)){
                    agent.SetPath(path);
                    yield return new WaitForSeconds(0.1f + Vector3.Distance(obj.transform.position, transform.position)*WAIT_FACTOR);
                }
                else{
                    yield return new WaitForSeconds(SEARCH_DELAY);
                }
            }
            else{
                yield return new WaitForSeconds(SEARCH_DELAY);
            }
        }
    }
}
