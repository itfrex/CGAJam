using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TNRD;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private const int PROJECTILE_CAP = 3000;
    public Transform cam;
    public Graphic cursor;
    public Rigidbody rb;
    private float yaw;
    private float pitch;
    public float sensitivity = 1;
    public float jumpStrength = 1;
    private Vector3 movement;
    private Vector3 kickback;
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
    public float speed = 1;
    public float firerateMult = 1;
    public float bulletDurationMult = 1;
    public float chaosMult = 1;
    public float knockbackMult = 1;
    public float accuracyMult = 1;
    private float timer;
    public bool randomizeSpellOrder = false;

    private float hurtTimer;
    public Volume renderingVolume;
    private Vignette vignette;
    public AnimationCurve vignetteCurve;
    public AudioLowPassFilter audioFilter;
    [SerializeField] AudioClip[] hurtSFX;
    private const float VIGNETTE_STRENGTH = 0.4f;
    private const float HEAL_TIME = 5f;
    private const float INVULN_TIME = 1f;
    private const float MIN_FREQ_CUTOFF = 1000;
    private const float MAX_FREQ_CUTOFF = 22000-MIN_FREQ_CUTOFF;
    
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
        GameController.Instance.SwapColor("_Magic", magicColors[magicIndex]);
        cursor.color = magicColors[magicIndex];
        kickback = Vector3.zero;
        renderingVolume.profile.TryGet(out vignette);
        vignette.intensity.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        yaw  += sensitivity*Input.GetAxis("Mouse X");
        pitch -= sensitivity*Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, pitchLower, pitchUpper);

        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, aimLayers)){
            aimDir = (hit.point-projSpawn.position).normalized.normalized;
            Debug.DrawLine(cam.position, hit.point);
            aimPoint = hit.point;
        }
        else{
            aimDir = (cam.transform.position + cam.transform.forward*100-projSpawn.position).normalized.normalized;
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
        if(Input.GetKeyDown("k")){
            Hurt();
        }
        timer += Time.deltaTime*chaosMult;
        if(timer >= 1){
            BumpMagic();
            timer = 0;
        }
        if (cooldown>0) cooldown -= Time.deltaTime*firerateMult;
        
        if(hurtTimer > 0){
            hurtTimer = Mathf.Max(0, hurtTimer - Time.deltaTime);
            vignette.intensity.value = VIGNETTE_STRENGTH*vignetteCurve.Evaluate(hurtTimer/HEAL_TIME);
            audioFilter.cutoffFrequency = MIN_FREQ_CUTOFF + MAX_FREQ_CUTOFF*(1-vignetteCurve.Evaluate(hurtTimer/HEAL_TIME));
        }
    }
    void FixedUpdate() {
        movement.y = rb.velocity.y;
        rb.velocity = transform.TransformDirection(movement) + kickback;
        if(jumpBuffer > 0 && isGrounded){
            rb.velocity += Vector3.up * jumpStrength;
            jumpBuffer = 0;
            isGrounded = false;
        }
        jumpBuffer = jumpBuffer > 0 ? 0 : jumpBuffer - 1;
        kickback *= 0.9f;
        if (kickback.magnitude < 0.1f){
            kickback = Vector3.zero;
        }
    }
    void OnCollisionEnter(Collision other) {
        if (jumpLayers == (jumpLayers | (1<< other.gameObject.layer))){
            isGrounded = true;
        }
    }
    private void CastWand(){
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
    }

    public Projectile[] DuplicateSpell(Projectile original, int count){
        Projectile[] clones = new Projectile[count];
        for(int i = 0; i < count; i++){
            if(pInactive.Count > 0){
                clones[i] = pInactive.Dequeue();
                clones[i].DeepCopy(original);
                clones[i].gameObject.SetActive(true);
            }
            else if(projectiles.Count+clonedProjectiles.Count < PROJECTILE_CAP){
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
        }
    }

    public ISpellBehaviour GetSpell(int i){
        return spells[i].Value;
    }
    public void SetCooldown(float amt){
        cooldown = amt;
    }
    public void BumpMagic(){
        if(randomizeSpellOrder) magicIndex = Random.Range(0,magicColors.Length);
        else magicIndex = (magicIndex+1)%magicColors.Length;
            foreach(Projectile p in projectiles){
                if(!p.isNewClone)p.SetState(magicIndex);
                else p.isNewClone = false;
            }
            while (clonedProjectiles.Count > 0){
                projectiles.Add(clonedProjectiles.Dequeue());
            }
        GameController.Instance.SwapColor("_Magic", magicColors[magicIndex]);
            cursor.color = magicColors[magicIndex];
    }
    public void ApplyKickback(float amt){
        kickback -= cam.transform.forward * amt * knockbackMult;
        rb.velocity += Vector3.up*kickback.y;
        kickback.y = 0;
    }
    public void Hurt(){
        if(hurtTimer > 0 && HEAL_TIME-hurtTimer > INVULN_TIME){
            GameController.Instance.Lose();
        }else if (hurtTimer <= 0){
            audioFilter.cutoffFrequency = MIN_FREQ_CUTOFF;
            AudioSource.PlayClipAtPoint(hurtSFX[Random.Range(0, hurtSFX.Length)], transform.position);
            hurtTimer = HEAL_TIME;
        }
    }
}
