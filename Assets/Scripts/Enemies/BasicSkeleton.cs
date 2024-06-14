using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BasicSkeleton : MonoBehaviour, IEnemy
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 1;
    private int id;
    private NavMeshAgent agent;
    private NavMeshPath path;
    private Coroutine searchRoutine;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private GameObject deathParticles;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        searchRoutine = StartCoroutine(CalculatePath(GameController.Instance.GetPlayer().gameObject));
    }

    public bool Spawn(int id){
        this.id = id;
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
        return Kill();
    }
    
    public void SetId(int id){
        this.id = id;
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.tag == "Player"){
            GameController.Instance.GetPlayer().Hurt();
        }
    }
    IEnumerator CalculatePath(GameObject obj){
        while(gameObject != null){
            if(agent.CalculatePath(obj.transform.position, path)){
                agent.SetPath(path);
                yield return new WaitForSeconds(0.1f + Vector3.Distance(obj.transform.position, transform.position)*WAIT_FACTOR);
            }
            else{
                yield return new WaitForSeconds(SEARCH_DELAY);
            }
        }
    }
}
