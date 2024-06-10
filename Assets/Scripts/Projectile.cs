using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public Rigidbody rb;
    public PlayerController player;
    public Collider col;
    private TrailRenderer trail;
    public ISpellBehaviour spell;
    public LayerMask collisionLayers;
    public LayerMask enemyLayers;
    public GameObject deathParticle;
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
        life = startLife;
        spell = player.GetSpell(state);
        gameObject.SetActive(true);
        spell.Cast(player,this);
    }
    private void OnCollisionEnter(Collision other) {
        spell.Hit(player,this,other);
        if(other.collider.CompareTag("Enemy")){
            other.gameObject.GetComponent<IEnemy>().Hit();
        }
    }
    public void SetState(int i){
        spell.SwitchOff(this);
        spell = player.GetSpell(i);
        spell.SwitchTo(this);
    }
}