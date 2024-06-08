using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TNRD;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cam;
    private Rigidbody rb;
    private float yaw;
    private float pitch;
    public float sensitivity = 1;
    public float speed = 1;
    public float jumpStrength = 1;
    private Vector3 movement;
    private float pitchUpper = 90;
    private float pitchLower = -90;
    private int jumpBuffer = 0;
    private bool isGrounded = true;
    private Vector3 groundNormal;

    public GameObject projectile;
    public Transform projSpawn;
    private List<Projectile> projectiles;
    private Queue<Projectile> pInactive;
    private int projCount = 0;
    public Color[] magicColors;
    private int magicIndex;
    [SerializeField] private SerializableInterface<ISpellBehaviour>[] spells;

    private Vector3 aimPoint;
    public LayerMask aimLayers;
    public Vector3 aimDir;
    
    void Start()
    {
        if(magicColors.Length != spells.Length){
            Debug.LogWarning("Color array not the same size as magic array! Will cause errors!");
        }
        rb = GetComponent<Rigidbody>();
        cam = transform.GetChild(0);
        groundNormal = Vector3.zero;
        projectiles = new List<Projectile>();
        pInactive = new Queue<Projectile>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        Shader.SetGlobalColor("_MagicColor", magicColors[magicIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        yaw  += sensitivity*Input.GetAxis("Mouse X");
        pitch -= sensitivity*Input.GetAxis("Mouse Y");
        pitch = Math.Clamp(pitch, pitchLower, pitchUpper);

        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, aimLayers)){
            aimDir = (hit.point-projSpawn.position).normalized;
            Debug.DrawLine(cam.position, hit.point);
            aimPoint = hit.point;
        }
        else{
            aimDir = cam.transform.forward;
        }
            Debug.DrawRay(projSpawn.position, aimDir*100, Color.green);

        cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        transform.eulerAngles = new Vector3(0, yaw, 0);

        movement = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"))*speed;
        
        if (Input.GetButtonDown("Jump")){
            jumpBuffer = 3;
        }
        if (Input.GetButtonDown("Fire1")){
            CastWand();
        }
        if(Input.GetButtonDown("Fire2")){
            magicIndex = (magicIndex+1)%magicColors.Length;
            foreach(Projectile p in projectiles){
                p.SetState(magicIndex);
            }
            Shader.SetGlobalColor("_MagicColor", magicColors[magicIndex]);
        }
    }
    void FixedUpdate() {
        movement.y = rb.velocity.y;
        rb.velocity = transform.TransformDirection(movement);
        if(jumpBuffer > 0){
            rb.velocity += Vector3.up * jumpStrength;
            jumpBuffer = 0;
            isGrounded = false;
        }
        jumpBuffer = jumpBuffer > 0 ? jumpBuffer - 1 : 0;
    }
    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Jumpable")){
            isGrounded = true;
        }
    }
    private bool CastWand(){
        Projectile p;
        if(pInactive.Count > 0){
            p = pInactive.Dequeue();
        }
        else{
            p = Instantiate(projectile).GetComponent<Projectile>();
            p.player=this;
            projectiles.Add(p);
        }
        p.Spawn(projSpawn.position, Quaternion.LookRotation(aimDir, projSpawn.transform.up), magicIndex);
        return true;
    }

    public void QueueInactive(Projectile p){
        pInactive.Enqueue(p);
    }

    public ISpellBehaviour GetSpell(int i){
        return spells[i].Value;
    }
}
