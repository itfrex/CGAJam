using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCrystal : MonoBehaviour, IEnemy
{
    [SerializeField] AudioClip[] hitSFX;
    [SerializeField] AudioClip[] breakSFX;
    [SerializeField] GameObject breakParticles;
    private Rigidbody rb;
    private Vector3 originalPos;
    private float heightOffset;
    private float timer;
    private int life;
    public GameObject beam;
    public void Start(){
        Spawn();
        originalPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }
    public bool Spawn(int id=0){
        GameController.Instance.crystalCount++;
        life = 5 * GameController.Instance.difficulty + 10;
        return true;
    }
    public bool Kill(){
        AudioSource.PlayClipAtPoint(breakSFX[Random.Range(0, breakSFX.Length)], transform.position);
        Destroy(Instantiate(breakParticles, transform.position, transform.rotation), 5);
        GameController.Instance.crystalCount--;
        if(GameController.Instance.crystalCount <= 0){
            GameController.Instance.WinStage();
        }
        Destroy(beam);
        Destroy(gameObject);
        return true;
    }
    public bool Hit(){
        AudioSource.PlayClipAtPoint(hitSFX[Random.Range(0, hitSFX.Length)], transform.position);
        life--;
        if (life == 0){
            Kill();
        }
        return true;
    }
    public void FixedUpdate(){
        rb.AddForce((originalPos-transform.position+Vector3.up*Mathf.Sin(timer*Mathf.PI)*0.3f)*150);
        rb.AddTorque(Vector3.up, ForceMode.Acceleration);
        timer = (timer + Time.deltaTime)%2;
    }
    public void SetId(int id){

    }
}
