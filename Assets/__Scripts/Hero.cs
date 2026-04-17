using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : MonoBehaviour
{

    public Weapon weapon;

    void Start()
    {
        Debug.Log(weapon.nextShotTime);
        // Capture the desired max volume from the AudioSource inspector value
        if (laserBeamAudioSource != null)
        {
            _laserBeamTargetVolume = laserBeamAudioSource.volume;
        }
    }


    public AudioSource audioSource;
    public AudioClip shotSound;
    public AudioClip powerUpAbsorbSound;
    public AudioClip heroHitSound;

    public AudioSource laserAudioSource;
    public AudioClip laserSound;

    public AudioSource laserBeamAudioSource;
    public AudioClip laserBeamSound;

    [Header("Audio Settings")]
    [Tooltip("Time in seconds to fade in the laser beam audio when Space is pressed")]
    public float laserBeamFadeInTime = 0.15f;

    // Runtime state for beam fade
    private Coroutine _laserBeamFadeCoroutine = null;
    private float _laserBeamTargetVolume = 1f;

    static public Hero S { get; private set; }  // Singleton property    // a

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public float powerUpVolume = 5f;
    public Weapon[] weapons;

    [Header("Camera Shake Settings")]
    public float duration = 1f; // Duration of the shake effect
    public float magnitude = 2f; // Magnitude of the shake effect

    public float fireshakeduration = 0.2f; // Duration of the shake effect when firing
    public float fireshakemagnitude = 0.1f; // Magnitude of the shake

    [Header("Dynamic")]
    [Range(0, 4)]
    [SerializeField]                                        // b
    private float _shieldLevel = 1;

    [Tooltip("This field holds a reference to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;

    // Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();                                // a     // Create a WeaponFireDelegate event named fireEvent.
    public event WeaponFireDelegate fireEvent;

    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton only if it’s null                  // c
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        //fireEvent += TempFire;

        // Reset the weapons to start _Hero with 1 blaster
        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }

    void Update()
    {
        // Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");                            // d
        float vAxis = Input.GetAxis("Vertical");                              // d

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic                       // e
        transform.rotation = Quaternion.Euler(vAxis * pitchMult, hAxis * rollMult, 0);

        // Beam audio: play looping beam while Space is held; stop immediately on release
        if (laserBeamAudioSource != null)
        {
            bool shouldPlayBeam = (weapon != null && weapon.type == eWeaponType.laser && Input.GetKey(KeyCode.Space));
            if (shouldPlayBeam)
            {
                StartBeam();
            }
            else
            {
                StopBeam();
            }
        }

        // Use the fireEvent to fire Weapons when the Spacebar is pressed.
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            if (Input.GetAxis("Jump") == 1 && fireEvent != null)
{
    // Only fire if the gun says it's allowed
    if (Time.time >= weapon.nextShotTime)

    {
        // Fire the shot
        fireEvent();
   
        // Update next shot time
        weapon.nextShotTime = Time.time + weapon.def.delayBetweenShots;

        if (weapon.type == eWeaponType.laser && Input.GetKey(KeyCode.Space))
        {
            if (!laserAudioSource.isPlaying || Input.GetKeyDown(KeyCode.Space))
                laserAudioSource.PlayOneShot(laserSound);
        }

        else

        // Play sound only once per allowed shot
        {
            audioSource.PlayOneShot(shotSound);
        }

        // Camera shake
        CameraShake.Instance.Shake(fireshakeduration, fireshakemagnitude);
    }
}
            {
            CameraShake.Instance.Shake(fireshakeduration, fireshakemagnitude);
        }
            
        }

    }


  


    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;                    // a
        GameObject go = rootT.gameObject;
        //Debug.Log("Shield trigger hit by: " + go.gameObject.name);

        // Make sure it’s not the same triggering go as last time
        if (go == lastTriggerGo) return;                                    // c
        lastTriggerGo = go;                                                   // d

        Enemy enemy = go.GetComponent<Enemy>();                               // e
        PowerUp pUp = go.GetComponent<PowerUp>();

        if (enemy != null)
        {  // If the shield was triggered by an enemy
            shieldLevel--;        // Decrease the level of the shield by 1
            Destroy(go);
            {
                CameraShake.Instance.Shake(duration, magnitude);
                audioSource.PlayOneShot(heroHitSound);
                //damage logic...
            }          // … and Destroy the enemy                  // f
        }
        else if (pUp != null)
        {
            AbsorbPowerUp(pUp);
        }
        else
        {
            Debug.LogWarning("Shield trigger hit by non-Enemy: " + go.name);    // g
        }
    }

    public float shieldLevel
    {
        get { return (_shieldLevel); }                                      // b
        private set
        {                                                         // c
            _shieldLevel = Mathf.Min(value, 4);                             // d
            // If the shield is going to be set to less than zero…
            if (value < 0)
            {   
                Main.HERO_DIED();                                               // e
                Destroy(this.gameObject);  // Destroy the Hero
                
            }
        }
    }

    /// <summary>
    /// Finds the first empty Weapon slot (i.e., type=none) and returns it.
    /// </summary>
    /// <returnsThe first empty Weapon slot or null if none are empty</returns
    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == eWeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    /// <summary>
    /// Sets the type of all Weapon slots to none
    /// </summary>
    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(eWeaponType.none);
        }
    }

    public void AbsorbPowerUp(PowerUp pUp)
    {
        audioSource.PlayOneShot(powerUpAbsorbSound, powerUpVolume);
        Debug.Log("Absorbed PowerUp: " + pUp.type);                         // b
        switch (pUp.type)
        {
            
            case eWeaponType.shield:                                              // a 
                shieldLevel++;
                break;

            default:                                                             // b
                if (pUp.type == weapons[0].type)
                { // If it is the same type     // c
                    Weapon weap = GetEmptyWeaponSlot();
                    if (weap != null)
                    {
                        // Set it to pUp.type
                        weap.SetType(pUp.type);
                    }
                }
                else
                { // If this is a different weapon type                   // d
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                }
                break;

        }
        pUp.AbsorbedBy(this.gameObject);
    }

    // Start the beam with a fade-in sound effect
    private void StartBeam()
    {
        if (laserBeamAudioSource == null) return;

        if (_laserBeamFadeCoroutine != null)
        {
            StopCoroutine(_laserBeamFadeCoroutine);
            _laserBeamFadeCoroutine = null;
        }

        if (!laserBeamAudioSource.isPlaying)
        {
            laserBeamAudioSource.clip = laserBeamSound;
            laserBeamAudioSource.loop = true;
            laserBeamAudioSource.volume = 0f;
            laserBeamAudioSource.Play();
        }

        _laserBeamFadeCoroutine = StartCoroutine(FadeAudioTo(laserBeamAudioSource, _laserBeamTargetVolume, laserBeamFadeInTime));
    }

    // Stop the beam immediately
    private void StopBeam()
    {
        if (laserBeamAudioSource == null) return;

        if (_laserBeamFadeCoroutine != null)
        {
            StopCoroutine(_laserBeamFadeCoroutine);
            _laserBeamFadeCoroutine = null;
        }

        if (laserBeamAudioSource.isPlaying)
        {
            laserBeamAudioSource.Stop();
            laserBeamAudioSource.loop = false;
            laserBeamAudioSource.volume = _laserBeamTargetVolume;
        }
    }

    private IEnumerator FadeAudioTo(AudioSource src, float target, float duration)
    {
        float start = src.volume;
        float t = 0f;
        if (duration <= 0f)
        {
            src.volume = target;
            _laserBeamFadeCoroutine = null;
            yield break;
        }
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        src.volume = target;
        _laserBeamFadeCoroutine = null;
    }

}
