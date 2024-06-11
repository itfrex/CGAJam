using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TNRD;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cam;
    public Graphic cursor;
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

    public GameObject projectile;
    public Transform projSpawn;
    private List<Projectile> projectiles;
    private Queue<Projectile> clonedProjectiles;
    private Queue<Projectile> pInactive;
    public Color[] magicColors;
    private int magicIndex;
    [SerializeField] private SerializableInterface<ISpellBehaviour>[] spells;

    private Vector3 aimPoint;
    public LayerMask aimLayers;
    public LayerMask jumpLayers;
    public Vector3 aimDir;

    private float cooldown;
    public float firerateMult;
    public float bulletDurationMult;
    public float chaosMult = 1;
    private float timer;
    
    [SerializeField] private AudioClip[] magicSounds;
    
    void Start()
    {
        if(magicColors.Length != spells.Length){
            Debug.LogWarning("Color array not the same size as magic array! Will cause errors!");
        }
        rb = GetComponent<Rigidbody>();
        cam = transform.GetChild(0);
        projectiles = new List<Projectile>();
        pInactive = new Queue<Projectile>();
        clonedProjectiles = new Queue<Projectile>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        Shader.SetGlobalColor("_MagicColor", magicColors[magicIndex]);
        cursor.color = magicColors[magicIndex];
    }

    // Update is called once per frame
    void Update()
    {
        yaw  += sensitivity*Input.GetAxis("Mouse X");
        pitch -= sensitivity*Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, pitchLower, pitchUpper);

        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, aimLayers)){
            aimDir = (hit.point-projSpawn.position).normalized;
            Debug.DrawLine(cam.position, hit.point);
            aimPoint = hit.point;
        }
        else{
            aimDir = (cam.transform.position + cam.transform.forward*100-projSpawn.position).normalized;
        }
            Debug.DrawRay(projSpawn.position, aimDir*100, Color.green);

        cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        transform.eulerAngles = new Vector3(0, yaw, 0);

        movement = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"))*speed;
        
        if (Input.GetButtonDown("Jump")){
            jumpBuffer = 3;
        }
        if (Input.GetButton("Fire1")){
            if(cooldown <= 0){
                CastWand();
            }
        }
        if(Input.GetButtonDown("Fire2")){
            BumpMagic();
        }
        timer += Time.deltaTime*chaosMult;
        if(timer >= 1){
            BumpMagic();
            timer = 0;
        }
        if (cooldown>0) cooldown -= Time.deltaTime*firerateMult;
    }
    void FixedUpdate() {
        movement.y = rb.velocity.y;
        rb.velocity = transform.TransformDirection(movement);
        if(jumpBuffer > 0 && isGrounded){
            rb.velocity += Vector3.up * jumpStrength;
            jumpBuffer = 0;
            isGrounded = false;
        }
        jumpBuffer = jumpBuffer > 0 ? 0 : jumpBuffer - 1;
    }
    void OnCollisionEnter(Collision other) {
        if (jumpLayers == (jumpLayers | (1<< other.gameObject.layer))){
            isGrounded = true;
        }
    }
    private bool CastWand(){
        Projectile p;
        AudioSource.PlayClipAtPoint(magicSounds[Random.Range(0, magicSounds.Length)], transform.position, 0.1f);
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

    public Projectile[] DuplicateSpell(Projectile original, int count){
        Projectile[] clones = new Projectile[count];
        for(int i = 0; i < count; i++){
            if(pInactive.Count > 0){
                clones[i] = pInactive.Dequeue();
                clones[i].DeepCopy(original);
                clones[i].gameObject.SetActive(true);
            }
            else{
                clones[i] = Instantiate(original).GetComponent<Projectile>();
                clones[i].player=this;
                clones[i].spell=spells[magicIndex].Value;
                clones[i].rb.velocity=original.rb.velocity;
                //need a seperate queue so we do not mess with enumeration during switching
                clonedProjectiles.Enqueue(clones[i]);
            }
        }
        return clones;
    }

    public void QueueInactive(Projectile p){
        if(projectiles.Count < 1000){
            pInactive.Enqueue(p);
        }else{
            projectiles.Remove(p);
            Destroy(p);
            Debug.Log(projectiles.Count);
        }
    }

    public ISpellBehaviour GetSpell(int i){
        return spells[i].Value;
    }
    public void SetCooldown(float amt){
        cooldown = amt;
    }
    public void BumpMagic(){
        magicIndex = (magicIndex+1)%magicColors.Length;
            foreach(Projectile p in projectiles){
                if(!p.isNewClone)p.SetState(magicIndex);
                else p.isNewClone = false;
            }
            while (clonedProjectiles.Count > 0){
                projectiles.Add(clonedProjectiles.Dequeue());
            }
            Shader.SetGlobalColor("_MagicColor", magicColors[magicIndex]);
            cursor.color = magicColors[magicIndex];
    }
}
