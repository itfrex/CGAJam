using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingSkull : MonoBehaviour, IEnemy
{
    private const float HOVER_HEIGHT = 1f;
    private const float ROT_SPEED = 0.75f;
    private const float SPEED = 10f;
    private Rigidbody rb;
    private float wobble;
    private Vector3 moveVec;
    [SerializeField] private LayerMask collisionLayers;
    public bool Spawn(){
        return true;
    }
    public bool Hit(){
        return true;
    }
    public bool Destroy(){
        return true;
    }

    void Start() {
        rb = GetComponent<Rigidbody>();   
    }
    void FixedUpdate() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, HOVER_HEIGHT, collisionLayers)){
            Debug.DrawRay(transform.position, Vector3.down*HOVER_HEIGHT, Color.red);
            rb.AddForce(Vector3.up * 10 * (1 - hit.distance/HOVER_HEIGHT));
        }
        rb.AddForce(Vector3.up * Mathf.Cos(wobble)*0.5f);
        moveVec = (GameController.GetPlayer().transform.position - transform.position).normalized;
        wobble += Time.fixedDeltaTime;
        
        Quaternion rot = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(moveVec, Vector3.up)*Quaternion.Inverse(rb.rotation),1);
        rb.AddTorque(new Vector3(rot.x, rot.y, rot.z)*ROT_SPEED);
        rb.AddForce(transform.forward*SPEED/(1+Vector3.Angle(transform.forward,moveVec)*0.2f));
    }
}
