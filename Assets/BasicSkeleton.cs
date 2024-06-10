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
    public PlayerController player;
    private Coroutine searchRoutine;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        searchRoutine = StartCoroutine(CalculatePath(player.gameObject));
    }

    public bool Spawn(){
        return true;
    }
    public bool Destroy(){
        return true;
    }
    public bool Hit(){
        return true;
    }

    IEnumerator CalculatePath(GameObject obj){
        while(true){
        Debug.Log("Searching...");
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
