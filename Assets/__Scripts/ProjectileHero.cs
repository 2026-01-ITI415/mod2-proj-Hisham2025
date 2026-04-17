using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour

{
    private BoundsCheck bndCheck;
    private Renderer rend;
    public Vector2 desiredDirection;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]                                                         // a
    private eWeaponType _type;


    // This public property masks the private field _type
    public eWeaponType type
    {                                              // c
        get { return (_type); }
        set { SetType(value); }
    }


    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();                                     // d
        rigid = GetComponent<Rigidbody>();
    }

void Update()
{
    if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
    {
        Destroy(gameObject);
        return;
    }

    Vector2 v = rigid.linearVelocity;

    if (v.sqrMagnitude > 0.01f)
    {
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        float targetZ = angle - 90f;

        float turnSpeed = 720f; // degrees per second (tweak this)

        float newZ = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            targetZ,
            turnSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0, 0, newZ);
    }
}
    /// <summary>
    /// Sets the _type private field and colors this projectile to match the 
    ///   WeaponDefinition.
    /// </summary>
    /// <param name="eType">The eWeaponType to use.</param>
    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.linearVelocity; }
        set { rigid.linearVelocity = value; }
    }

}
