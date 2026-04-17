using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum eWeaponType
{
    none,
    blaster,
    spread,
    phaser,
    missile,
    heatSeeker,
    laser,
    shield
}

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    public string letter;
    public Color powerUpColor = Color.white;
    public GameObject weaponModelPrefab;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float damagePerSec = 0;
    public float delayBetweenShots = 0;
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    private eWeaponType _type = eWeaponType.none;

    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    private Transform shotPointTrans;

private void FireLaserInstant()
{
    float range = 100f;

    Vector3 start = shotPointTrans.position;
    Vector3 dir = Vector3.up;

    RaycastHit[] hits = Physics.RaycastAll(start, dir, range);
    Vector3 end = start + dir * range;

    if (hits.Length > 0)
    {
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool hitFound = false;
        foreach (RaycastHit h in hits)
        {
            if (h.collider.GetComponentInParent<PowerUp>() != null)
            {
                continue;
            }

            hitFound = true;
            end = h.point;

            // ENEMY CHECK
            if (h.collider.CompareTag("Enemy"))
            {
                Enemy e = h.collider.GetComponentInParent<Enemy>();
                if (e != null)
                {
                    // DPS DAMAGE
                    e.TakeDamage(def.damagePerSec * Time.deltaTime);

                    // Start blinking this enemy while laser is hitting it
                    if (currentLaserTarget == null || currentLaserTarget != e)
                    {
                        if (currentLaserTarget != null) currentLaserTarget.StopLaserBlink();
                        currentLaserTarget = e;
                        currentLaserTarget.StartLaserBlink();
                    }
                }
            }
            else
            {
                // Ray hit something else — stop blinking previous target
                if (currentLaserTarget != null)
                {
                    currentLaserTarget.StopLaserBlink();
                    currentLaserTarget = null;
                }
            }

            break;
        }

        if (!hitFound)
        {
            end = start + dir * range;
            if (currentLaserTarget != null)
            {
                currentLaserTarget.StopLaserBlink();
                currentLaserTarget = null;
            }
        }
    }
    else
    {
        // MISS → full laser length
        end = start + dir * range;
        if (currentLaserTarget != null)
        {
            currentLaserTarget.StopLaserBlink();
            currentLaserTarget = null;
        }
    }

    // LASER VISUAL
    laser.SetPosition(0, start);
    laser.SetPosition(1, end);

    laser.enabled = true;

    // Short burst style (prevents stacking visual artifacts)
    CancelInvoke(nameof(DisableLaser));
    Invoke(nameof(DisableLaser), 0.05f);

    nextShotTime = Time.time + def.delayBetweenShots;
}

void OnTriggerStay(Collider other)
{
    Enemy e = other.GetComponentInParent<Enemy>();
    if (e != null)
    {
        e.TakeDamage(def.damagePerSec * Time.deltaTime);
    }
}

private void DisableLaser()
{
    laser.enabled = false;
    if (currentLaserTarget != null)
    {
        currentLaserTarget.StopLaserBlink();
        currentLaserTarget = null;
    }
}

    void Start()
{
    if (PROJECTILE_ANCHOR == null)
    {
        GameObject go = new GameObject("_ProjectileAnchor");
        PROJECTILE_ANCHOR = go.transform;
    }

    shotPointTrans = transform.GetChild(0);
    SetType(_type);

    // CREATE LASER ONCE
    laserGO = new GameObject("LaserBeam");
    laserGO.transform.SetParent(PROJECTILE_ANCHOR);

    laser = laserGO.AddComponent<LineRenderer>();
    laser.positionCount = 2;
    laser.startWidth = 0.2f;
    laser.endWidth = 0.05f;
    laser.material = new Material(Shader.Find("Sprites/Default"));
    laser.material.renderQueue = 4000;
    laser.material.SetInt("_ZWrite", 0);
    laser.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    laser.startColor = Color.red;
    laser.endColor = new Color(1, 0.5f, 0.5f);
    laser.enabled = false;

    Hero hero = GetComponentInParent<Hero>();
    if (hero != null) hero.fireEvent += Fire;

}

    // HEAT SEEKER
    private IEnumerator HeatSeekRoutine(ProjectileHero proj)
    {
        float speed = def.velocity;
        float turnSpeed = 8000f;
        float lifeTime = 5f;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Transform target = AcquireClosestEnemy(proj.transform.position);

        if (target == null)
        {
            Destroy(proj.gameObject);
            yield break;
        }

        float timer = 0f;

        while (proj != null && timer < lifeTime)
        {
            timer += Time.deltaTime;

            if (target == null)
                target = AcquireClosestEnemy(proj.transform.position);

            if (target == null)
            {
                Destroy(proj.gameObject);
                yield break;
            }

            if (target != null)
            {
                Vector3 dir = (target.position - proj.transform.position).normalized;

                // Rotate toward target using Rigidbody (NO jitter)
                Quaternion targetRot = Quaternion.LookRotation(dir);

                Quaternion newRot = Quaternion.RotateTowards(
                    rb.rotation,
                    targetRot,
                    turnSpeed * Time.deltaTime
                );

                rb.MoveRotation(newRot);
            }

            // Move forward consistently
            rb.linearVelocity= rb.rotation * Vector3.forward * speed;

            yield return null;
        }

        if (proj != null)
            Destroy(proj.gameObject);
    }

    private Transform AcquireClosestEnemy(Vector3 fromPos)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject e in enemies)
        {
            float d = (e.transform.position - fromPos).sqrMagnitude;
            if (d < closestDist)
            {
                closestDist = d;
                closest = e.transform;
            }
        }

        return closest;
    }

    private LineRenderer laser;
    private GameObject laserGO;
    // Track current enemy hit by the instant laser so we can stop its blink
    private Enemy currentLaserTarget;

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;

        if (type == eWeaponType.none)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        def = Main.GET_WEAPON_DEFINITION(_type);

        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0;
    }
        private void Fire()
    {
        if (!gameObject.activeInHierarchy) return;
        if (Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile();
                p.vel = vel;

                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;

                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;

            case eWeaponType.heatSeeker:
                p = MakeProjectile();

                // Initial push
                p.vel = vel;

                // Start homing behavior
                StartCoroutine(HeatSeekRoutine(p));
                break;

            case eWeaponType.laser:
            FireLaserInstant();
            break;
        }
    }

    private ProjectileHero MakeProjectile()
    {
        GameObject go = Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;

        return p;
    }
}



