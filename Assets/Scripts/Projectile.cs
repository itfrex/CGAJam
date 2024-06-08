using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public Rigidbody rb;
    public PlayerController player;
    public Collider col;
    private TrailRenderer trail;
    private ISpellBehaviour spell;
    public LayerMask collisionLayers;
    private int startLife = 100;
    public int life;
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
        spell.Sustain(player,this);
        life -= 1;
        if (life <= 0){
            gameObject.SetActive(false);
            player.QueueInactive(this);
        }
    }
    public void Spawn(Vector3 position, Quaternion rotation, int state){
        transform.position = position;
        transform.rotation = rotation;
        trail.Clear();
        life = startLife;
        spell = player.GetSpell(state);
        Debug.Log(transform.forward);
        spell.Cast(player,this);
        gameObject.SetActive(true);
    }
    private void OnCollisionEnter(Collision other) {
        spell.Hit(player,this,other);
    }
    public void SetState(int i){
        spell.SwitchOff(this);
        spell = player.GetSpell(i);
        spell.SwitchTo(this);
    }
}
