using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip enemyDeathSound;

    public GameObject explosionPrefab;


    [Header("Inscribed")]
    public float speed = 10f;   // The movement speed is 10m/s
    public float fireRate = 0.3f;  // Seconds/shot (Unused)
    public float health = 10;    // Damage needed to destroy this enemy
    public int score = 100;   // Points earned for destroying this
    public float powerUpDropChance = 1f;
    public float deathVolume = 0.25f;


    // private BoundsCheck bndCheck;                                             // b
    protected BoundsCheck bndCheck;
    protected bool calledShipDestroyed = false;
    // Laser blink support using BlinkColorOnHit
    private BlinkColorOnHit[] blinkComps;
    private Coroutine laserBlinkCoroutine;
    public float laserBlinkInterval = 0.5f; // total cycle interval

    void Awake()
    {                                                            // c
        bndCheck = GetComponent<BoundsCheck>();
        blinkComps = GetComponentsInChildren<BlinkColorOnHit>();
    }

public void TakeDamage(float dmg)
{
    health -= dmg;

    if (health <= 0)
    {
        if (!calledShipDestroyed)
        {
            calledShipDestroyed = true;
            Main.SHIP_DESTROYED(this);
        }

        // EXPLOSION
        Instantiate(explosionPrefab, transform.position, transform.rotation);

        // SOUND
        GameObject audioObj = new GameObject("EnemyDeathSound");
        AudioSource aSource = audioObj.AddComponent<AudioSource>();
        aSource.clip = enemyDeathSound;
        aSource.PlayOneShot(enemyDeathSound, deathVolume);
        Destroy(audioObj, enemyDeathSound.length);

        Destroy(gameObject);
    }
}



    // This is a Property: A method that acts like a field
    public Vector3 pos
    {                                                       // a
        get
        {
            return this.transform.position;
        }
        set
        {
            this.transform.position = value;
        }
    }

    void Update()
    {
        Move();

        // Check whether this Enemy has gone off the bottom of the screen
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            Destroy(gameObject);


        }
    }

    public virtual void Move()
    { // c
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;

        // Check for collisions with ProjectileHero
        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        LineRenderer lr = otherGO.GetComponent<LineRenderer>();
        if (p != null)
        {                                                  
            // Only damage this Enemy if it’s on screen
            if (bndCheck.isOnScreen)
            {                                      
                // Get the damage amount from the Main WEAP_DICT.
                TakeDamage(Main.GET_WEAPON_DEFINITION(p.type).damageOnHit);
                if (health <= 0)
                {
                    if (!calledShipDestroyed)
                    {
                        calledShipDestroyed = true;
                        
                        Main.SHIP_DESTROYED(this);
                    }

                    Destroy(this.gameObject);
                    

                    Instantiate(explosionPrefab, transform.position, transform.rotation);

                    GameObject audioObj = new GameObject("EnemyDeathSound");     // d
                    AudioSource audioSource = audioObj.AddComponent<AudioSource>();
                    audioSource.clip = enemyDeathSound;
                    audioSource.PlayOneShot(enemyDeathSound, deathVolume);

                    Destroy(audioObj, enemyDeathSound.length);
                    GameObject fx = Instantiate(explosionPrefab, transform.position, transform.rotation);
                    Destroy(fx, 1f);;

                
                // a

                    
                    
                }
            }
            // Destroy the ProjectileHero regardless
            Destroy(otherGO);                         // e
        }
        else if (lr != null)
        {
            // Laser began colliding: start repeating the blink
            StartLaserBlink();
        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);      // f
        }
    }

    void OnCollisionExit(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        LineRenderer lr = otherGO.GetComponent<LineRenderer>();
        if (lr != null)
        {
            StopLaserBlink();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        LineRenderer lr = other.GetComponent<LineRenderer>();
        if (lr != null)
        {
            StartLaserBlink();
        }
    }

    void OnTriggerExit(Collider other)
    {
        LineRenderer lr = other.GetComponent<LineRenderer>();
        if (lr != null)
        {
            StopLaserBlink();
        }
    }

    public void StartLaserBlink()
    {
        if (laserBlinkCoroutine == null && blinkComps.Length > 0)
        {
            laserBlinkCoroutine = StartCoroutine(LaserBlinkCoroutine());
        }
    }

    public void StopLaserBlink()
    {
        if (laserBlinkCoroutine != null)
        {
            StopCoroutine(laserBlinkCoroutine);
            laserBlinkCoroutine = null;
        }

        foreach (var bc in blinkComps)
        {
            if (bc != null && bc.showingColor)
            {
                bc.RevertColors();
            }
        }
    }

    IEnumerator LaserBlinkCoroutine()
    {
        while (true)
        {
            // Trigger blink (BlinkColorOnHit uses its internal blinkDuration, default 0.1s)
            foreach (var bc in blinkComps)
            {
                if (bc != null) bc.SetColors();
            }
            yield return new WaitForSeconds(laserBlinkInterval);
        }
    }

}