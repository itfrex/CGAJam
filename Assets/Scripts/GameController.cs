using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
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
    public GameObject[] aliveEnemies;
    private int enemyCount;
    private int enemyCap = 100;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        player = GameObject.FindObjectOfType<PlayerController>();
        aliveEnemies = new GameObject[enemyCap];
        StartCoroutine(SpawnLoop());
    }
    public static PlayerController GetPlayer(){
        return player;
    }
    private IEnumerator SpawnLoop(){
        while(doSpawning && enemyCount < enemyCap){
            Vector3 point;
            if(RandomPoint(player.transform.position, 30, out point)){
                yield return new WaitForEndOfFrame();
                GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], point, Quaternion.identity);
                enemy.GetComponent<IEnemy>().Spawn(enemyCount);
                aliveEnemies[enemyCount] = enemy;
                enemyCount++;
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
    public void RemoveEnemy(int id){
        aliveEnemies[id] = aliveEnemies[enemyCount-1];
        aliveEnemies[id].GetComponent<IEnemy>().SetId(id);
        enemyCount--;
    }
    public GameObject GetRandomEnemy(){
        return aliveEnemies[Random.Range(0, enemyCount)];
    }
    public GameObject GetNearestEnemy(Vector3 point){
        float best = Mathf.Infinity;
        int bestIndex = 0;
        for (int i = 0; i < enemyCount; i++){
            if(best > Vector3.Distance(point, aliveEnemies[i].transform.position)){
                best = Vector3.Distance(point, aliveEnemies[i].transform.position);
                bestIndex = i;
            }
        }
        return aliveEnemies[bestIndex];
    }
}
