using UnityEngine;

public class HeatSeeker : MonoBehaviour
{
    public float speed = 20f;
    public float turnSpeed = 1000f;
    public float lifeTime = 20f;

    private Rigidbody rb;
    private Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
        AcquireTarget();

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
    }

void FixedUpdate()
{
    if (target == null)
    {
        rb.linearVelocity = transform.forward * speed;
        return;
    }

    Vector3 dir = (target.position - transform.position).normalized;
    Quaternion targetRot = Quaternion.LookRotation(dir); // Flip to face the target

    Quaternion newRot = Quaternion.RotateTowards(
        rb.rotation,
        targetRot,
        turnSpeed * Time.fixedDeltaTime
    );

    rb.MoveRotation(newRot);

    // Apply velocity using the NEW forward direction
    rb.linearVelocity = newRot * Vector3.forward * speed;
}




    void AcquireTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject e in enemies)
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < closestDist)
            {
                closestDist = d;
                closest = e.transform;
            }
        }

        target = closest;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}