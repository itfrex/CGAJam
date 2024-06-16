using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEditor;


public class GameController : MonoBehaviour
{
    public const float BPM = 150;
    private const int ARTIFACT_SCENE_INDEX = 1;
    private const int WIN_SCENE_INDEX = 6;
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
    public AudioSource audioSource;
    private PlayerController player;
    public bool doSpawning;
    public GameObject[] enemies;
    public GameObject[] aliveEnemies;
    private int enemyCount;
    private int enemyCap = 100;
    public int crystalCount;
    public List<Artifact> artifacts;
    private float enemySpawnTime = 5f;
    private int navMesh;
    public int difficulty;
    private int stageNumber;
    private bool hiContrast;
    [SerializeField] private int[] stageOrder;

    private void Awake() {
            if(_instance != null && _instance != this){
                Destroy(this.gameObject);
                return;
            }else{
                _instance = this;
            }
        SceneManager.sceneLoaded += StateOpener;
        hiContrast = paletteSwapEffect.passMaterial.GetColor("_White") == Color.white;
        DontDestroyOnLoad(gameObject);
    }
    public void InitGameplay(){
    navMesh = NavMesh.GetAreaFromName("Flying");
    StartCoroutine(DeathFadeOut());
    StartCoroutine(BlackFadeOut(0.05f));
    player = GameObject.FindObjectOfType<PlayerController>();
    aliveEnemies = new GameObject[enemyCap];
    enemyCount = 0;
    foreach (Artifact a in artifacts){
        foreach (Artifact.Effect e in a.effects){
            e.effect.Value.ApplyEffect(e.amt);
        }
    }
    }
    public void StartLevel(){
        Debug.Log("Game Start!");
        enemySpawnTime = 4/difficulty;
        doSpawning = true;
        StartCoroutine(SpawnLoop());
    }
    public PlayerController GetPlayer(){
        return player;
    }
    private IEnumerator SpawnLoop(){
        Debug.Log("Spawn loop started with difficulty " + difficulty);
        while(doSpawning){
            Vector3 point;
            if(enemyCount < enemyCap && RandomPoint(player.transform.position, 30, out point)){
                yield return new WaitForEndOfFrame();
                GameObject enemy = Instantiate(enemies[UnityEngine.Random.Range(0, Mathf.Min(difficulty,enemies.Length))], point, Quaternion.identity);
                enemy.GetComponent<IEnemy>().Spawn(enemyCount);
                aliveEnemies[enemyCount] = enemy;
                enemyCount++;
            }
            yield return new WaitForSeconds(enemySpawnTime);
            enemySpawnTime = Mathf.Max(1/difficulty, enemySpawnTime*0.99f);
        }
    }

    // from unity docs: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            float rand = UnityEngine.Random.Range(0,360);
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
        return aliveEnemies[UnityEngine.Random.Range(0, enemyCount)];
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
        stageNumber++;
        if(stageNumber >= stageOrder.Length){
            WinGame();
            return;
        }
        StartCoroutine(BlackFadeIn(0.05f, ARTIFACT_SCENE_INDEX));
    }
    public void ProceedStage(){
        StopAllCoroutines();
        difficulty += 1;
        StartCoroutine(BlackFadeIn(0.05f, stageOrder[Mathf.Min(stageOrder.Length, stageNumber)]));
    }
    public void StartGame(){
        difficulty = 0;
        stageNumber = 0;
        crystalCount = 0;
        artifacts = new List<Artifact>();
        audioSource.Stop();
        AudioManager.instance.Play("StartGame");
        audioSource.clip = AudioManager.instance.GetClip("GameMusic");
        audioSource.PlayScheduled(AudioSettings.dspTime + 1f);
        StartCoroutine(BlackFadeIn(0.05f, stageOrder[difficulty]));
    }
    public void Lose(){
        StopAllCoroutines();
        crystalCount = 0;
        //artifacts = new List<Artifact>();
        doSpawning = false;
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
        SceneManager.LoadScene(stageOrder[stageNumber]);
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
    public void WinGame(){
        StartCoroutine(BlackFadeIn(0.05f, WIN_SCENE_INDEX));
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
        if(scene.buildIndex == 0){
            Cursor.lockState = CursorLockMode.None;
            audioSource.Stop();
            audioSource.clip = AudioManager.instance.GetClip("Menu");
            audioSource.Play();
            SetDitherEffect(true);
        }else if(stageOrder.Contains(scene.buildIndex)){
            SetDitherEffect(true);
            InitGameplay();
            if(difficulty > 0) StartLevel();
        }else if(scene.buildIndex == ARTIFACT_SCENE_INDEX) {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            GameController.Instance.SetDitherEffect(false);
        }else if(scene.buildIndex == WIN_SCENE_INDEX) {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            audioSource.Stop();
            audioSource.clip = AudioManager.instance.GetClip("Win");
            audioSource.Play();
            GameController.Instance.SetDitherEffect(false);
        }
    }
    public bool GetPlayerNavPoint(out Vector3 point){
        NavMeshHit hit; 
            if(NavMesh.SamplePosition(player.transform.position, out hit, 10, navMesh)){
                point = hit.position;
                return true;
            }else{
                point = Vector3.zero;
                return false;
            }
    }    public void SwitchContrast(){
        hiContrast = !hiContrast;
        if(hiContrast){
            SwapColor("_White", Color.white);
        }else{
            SwapColor("_White", new Color(2f/3f,2f/3f,2f/3f));
        }
    }
    public void ChangeVolume(){
        AudioListener.volume = UnityEngine.Random.Range(0,1f);
    }
    public void ExitToMenu(){
        StopAllCoroutines();
        StartCoroutine(BlackFadeIn(0.5f, 0));
    }
}
