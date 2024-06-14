using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance{
        get{
            return _instance;
        }
    }
    private PlayerController player;
    public bool doSpawning;
    public GameObject[] enemies;
    public GameObject[] aliveEnemies;
    private int enemyCount;
    private int enemyCap = 100;
    public int crystalCount;
    public List<Artifact> artifacts;
    private Coroutine enemySpawnRoutine;
    private float enemySpawnTime = 1f;

    private void Awake() {
            if(_instance != null && _instance != this){
                Destroy(this.gameObject);
            }else{
                _instance = this;
            }
        DontDestroyOnLoad(gameObject);
        
    }
    public void StartLevel(){
        player = GameObject.FindObjectOfType<PlayerController>();
        aliveEnemies = new GameObject[enemyCap];
        enemyCount = 0;
        enemySpawnRoutine = StartCoroutine(SpawnLoop());
        foreach (Artifact a in artifacts){
            foreach (Artifact.Effect e in a.effects){
                e.effect.Value.ApplyEffect(e.amt);
            }
        }
    }
    public PlayerController GetPlayer(){
        return player;
    }
    private IEnumerator SpawnLoop(){
        while(doSpawning){
            Vector3 point;
            if(enemyCount < enemyCap && RandomPoint(player.transform.position, 30, out point)){
                yield return new WaitForEndOfFrame();
                GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], point, Quaternion.identity);
                enemy.GetComponent<IEnemy>().Spawn(enemyCount);
                aliveEnemies[enemyCount] = enemy;
                enemyCount++;
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(1);
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
                    result = Vector3.zero;
                    return false;
                }
            }
        }
        result = Vector3.zero;
        return false;
    }
    public void RemoveEnemy(int id){
        if(id >= 0){
            aliveEnemies[id].GetComponent<IEnemy>().SetId(-1);
            aliveEnemies[id] = aliveEnemies[enemyCount-1];
            aliveEnemies[id].GetComponent<IEnemy>().SetId(id);
            enemyCount--;
        }
    }
    public GameObject GetRandomEnemy(){
        return aliveEnemies[Random.Range(0, enemyCount)];
    }
    public GameObject GetNearestEnemy(Vector3 point){
        float best = Mathf.Infinity;
        int bestIndex = 0;
        for (int i = 0; i < enemyCount; i++){
            float dist = Vector3.Distance(point, aliveEnemies[i].transform.position);
            if(best > dist){
                best = dist;
                bestIndex = i;
            }
        }
        return aliveEnemies[bestIndex];
    }
    public void Win(){
        Debug.Log("YOU WIN");
        StopAllCoroutines();
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene(1);
    }
}
