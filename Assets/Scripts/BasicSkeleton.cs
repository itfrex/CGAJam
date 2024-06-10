using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicSkeleton : MonoBehaviour, IEnemy
{
    private const float WAIT_FACTOR = 0.1f;
    private const float SEARCH_DELAY = 1;
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
        searchRoutine = StartCoroutine(CalculatePath(GameController.GetPlayer().gameObject));
    }

    public bool Spawn(){
        return true;
    }
    public bool Destroy(){
        return true;
    }
    public bool Hit(){
        AudioSource.PlayClipAtPoint(deathSounds[Random.Range(0, deathSounds.Length)], transform.position);
        Destroy(Instantiate(deathParticles, transform.position, transform.rotation), 5);
        StopCoroutine(searchRoutine);
        Destroy(gameObject);
        return true;
    }

    IEnumerator CalculatePath(GameObject obj){
        while(true){
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
