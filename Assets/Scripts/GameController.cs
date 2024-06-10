using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance{
        get{
            if(_instance == null){
                _instance = GameObject.FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }
    private static PlayerController player;
    public bool doSpawning;
    public GameObject[] enemies;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        player = GameObject.FindObjectOfType<PlayerController>();
        StartCoroutine(SpawnLoop());
    }
    public static PlayerController GetPlayer(){
        return player;
    }
    private IEnumerator SpawnLoop(){
        while(doSpawning){
            Vector3 point;
            if(RandomPoint(player.transform.position, 30, out point)){
                IEnemy enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], point, Quaternion.identity).GetComponent<IEnemy>();
                enemy.Spawn();
                yield return new WaitForSeconds(1);
            }
        }
    }

    // from unity docs: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            float rand = Random.Range(0,360);
            Vector3 randomPoint = center + new Vector3(Mathf.Cos(rand),0,Mathf.Sin(rand)) * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            else{
                range *= 0.9f;
                if (range < 10){
                    break;
                }
            }
        }
        result = Vector3.zero;
        return false;
    }
}
