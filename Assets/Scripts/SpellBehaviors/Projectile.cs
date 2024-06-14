using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{

    public bool isNewClone = false;
    public Rigidbody rb;
    public PlayerController player;
    public Collider col;
    private TrailRenderer trail;
    public ISpellBehaviour spell;
    public LayerMask collisionLayers;
    public LayerMask enemyLayers;
    public GameObject deathParticle;
    public GameObject target; //this is only here because i didnt realize Interfaces were not instanced
    public NavMeshPath path;
    public Vector3 dir;
    private float startLife = 100;
    public float life;
    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        trail = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate(){
        if (life <= 0){
            spell.End(player, this);
            GameObject particle = Instantiate(deathParticle, transform.position, transform.rotation);
            particle.GetComponent<Rigidbody>().velocity = rb.velocity;
            particle.SetActive(true);
            Destroy(particle,1);
            gameObject.SetActive(false);
            player.QueueInactive(this);
        }
        spell.Sustain(player,this);
    }
    public void Spawn(Vector3 position, Quaternion rotation, int state){
        transform.position = position;
        transform.rotation = rotation;
        trail.Clear();
        life = startLife*player.bulletDurationMult;
        spell = player.GetSpell(state);
        gameObject.SetActive(true);
        spell.Cast(player,this);
    }
    private void OnCollisionEnter(Collision other) {
        if(other.collider.CompareTag("Enemy")){
            other.gameObject.GetComponent<IEnemy>().Hit();
        }
        spell.Hit(player,this,other);
    }
    private void OnCollisionStay(Collision other) {
        if(other.collider.CompareTag("Enemy")){
            other.gameObject.GetComponent<IEnemy>().Hit();
        }
        spell.Hit(player,this,other);
    }
    public void SetState(int i){
        if(gameObject.activeInHierarchy)spell.SwitchOff(this);
        spell = player.GetSpell(i);
        if(gameObject.activeInHierarchy)spell.SwitchTo(this);
    }
    public void DeepCopy(Projectile p){
        rb.velocity = p.rb.velocity;
        rb.angularVelocity = p.rb.angularVelocity;
        transform.position = p.transform.position;
        transform.rotation = p.transform.rotation;
        spell = p.spell;
        life = p.life;
        target = p.target;
        dir = p.dir;
        path = p.path;
        isNewClone = true;
    }
}
