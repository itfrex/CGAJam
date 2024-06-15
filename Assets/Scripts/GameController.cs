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
    [SerializeField] FullScreenPassRendererFeature deathEffect;
    [SerializeField] FullScreenPassRendererFeature blackFadeEffect;
    [SerializeField] FullScreenPassRendererFeature quantizerEffect;
    [SerializeField] FullScreenPassRendererFeature paletteSwapEffect;
    private PlayerController player;
    public bool doSpawning;
    public GameObject[] enemies;
    public GameObject[] aliveEnemies;
    private int enemyCount;
    private int enemyCap = 100;
    public int crystalCount;
    public List<Artifact> artifacts;
    private float enemySpawnTime = 2f;
    public int gameState;

    private void Awake() {
            if(_instance != null && _instance != this){
                Destroy(this.gameObject);
                return;
            }else{
                _instance = this;
                StartLevel();
            }
        SceneManager.sceneLoaded += StateOpener;
        DontDestroyOnLoad(gameObject);
    }
    public void StartLevel(){
        SetDitherEffect(true);
        StartCoroutine(DeathFadeOut());
        StartCoroutine(BlackFadeOut(0.05f));
        player = GameObject.FindObjectOfType<PlayerController>();
        aliveEnemies = new GameObject[enemyCap];
        enemyCount = 0;
        doSpawning = true;
        StartCoroutine(SpawnLoop());
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
            }
            yield return new WaitForSeconds(enemySpawnTime);
            enemySpawnTime = Mathf.Max(0.1f, enemySpawnTime*0.99f);
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
    public void WinStage(){
        StopAllCoroutines();
        gameState = 1;
        StartCoroutine(BlackFadeIn(0.05f, 1));
    }
    public void ProceedStage(){
        StopAllCoroutines();
        gameState = 0;
        StartCoroutine(BlackFadeIn(0.05f, 0));
    }
    public void Lose(){
        StopAllCoroutines();
        crystalCount = 0;
        artifacts = new List<Artifact>();
        doSpawning = false;
        enemySpawnTime = 2;
        StartCoroutine(DeathFadeIn());
    }
    
    IEnumerator DeathFadeOut(){
        float fade = deathEffect.passMaterial.GetFloat("_DeathFadeValue");
        while (fade > 0){
            deathEffect.passMaterial.SetFloat("_DeathFadeValue", fade);
            fade = Mathf.Max(0, fade - 0.05f);
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    IEnumerator DeathFadeIn(){
        float fade = deathEffect.passMaterial.GetFloat("_DeathFadeValue");;
        while (fade < 1){
            deathEffect.passMaterial.SetFloat("_DeathFadeValue", fade);
            fade = Mathf.Min(1, fade + 0.02f);
            yield return new WaitForSeconds(0.05f);
        }
        SceneManager.LoadScene(0);
        StartCoroutine(DeathFadeOut());
    }
    IEnumerator BlackFadeOut(float speed){
        float fade = blackFadeEffect.passMaterial.GetFloat("_FadeValue");
        while (fade > 0){
            blackFadeEffect.passMaterial.SetFloat("_FadeValue", fade);
            fade = Mathf.Max(0, fade - speed);
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    IEnumerator BlackFadeIn(float speed, int scene){
        float fade = blackFadeEffect.passMaterial.GetFloat("_FadeValue");;
        while (fade < 1){
            blackFadeEffect.passMaterial.SetFloat("_FadeValue", fade);
            fade = Mathf.Min(1, fade + speed);
            yield return new WaitForSeconds(0.05f);
        }
        SceneManager.LoadScene(scene);
        StartCoroutine(BlackFadeOut(speed));
    }
    public void SwapColor(string name, Color col){
        paletteSwapEffect.passMaterial.SetColor(name, col);
    }
    public void SetDitherEffect(bool active){
        quantizerEffect.SetActive(active);
    }
    private void StateOpener(Scene scene, LoadSceneMode mode){
        switch(scene.buildIndex){
            case(0): {
                StartLevel();
                break;
            }
            case(1): {
                UnityEngine.Cursor.lockState = CursorLockMode.Confined;
                GameController.Instance.SetDitherEffect(false);
                break;
            }
        }
    }
}
