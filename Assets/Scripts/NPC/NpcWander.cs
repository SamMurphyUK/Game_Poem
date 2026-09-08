using UnityEngine;

// Picks a random heading, walks for a bit, idles, and turns away from walls.
// Combine takes the body over; wander waits until the pair splits again.
public class NpcWander : MonoBehaviour
{
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float minWalkTime = 1.2f;
    [SerializeField] private float maxWalkTime = 4f;
    [SerializeField] private float minIdleTime = 0.35f;
    [SerializeField] private float maxIdleTime = 1.8f;
    [SerializeField] private float wanderRadius = 0f;
    [SerializeField] private Transform wanderOrigin;
    [SerializeField] private bool faceMoveDirection = true;

    private Rigidbody2D body;
    private CombinerComponent combiner;
    private Vector2 direction = Vector2.right;
    private float speed;
    private float stateTimer;
    private bool walking;

    public void ConfigureHome(Transform origin, float radius)
    {
        wanderOrigin = origin;
        wanderRadius = Mathf.Max(0f, radius);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        combiner = GetComponent<CombinerComponent>();
        PickIdle();
    }

    private void FixedUpdate()
    {
        if (body == null)
        {
            return;
        }

        if (combiner != null && combiner.IsBusy())
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f)
        {
            if (walking)
            {
                PickIdle();
            }
            else
            {
                PickWalk();
            }
        }

        Vector2 home = wanderOrigin != null ? (Vector2)wanderOrigin.position : (Vector2)transform.position;
        Vector2 steered = NpcWanderMath.Steer(transform.position, home, wanderRadius, walking ? direction : Vector2.zero);

        if (walking)
        {
            direction = steered.sqrMagnitude > 0.0001f ? steered : direction;
            body.linearVelocity = direction * speed;
            FaceDirection();
        }
        else
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0)
        {
            return;
        }

        direction = NpcWanderMath.Bounce(direction, collision.GetContact(0).normal);
        if (!walking)
        {
            PickWalk();
        }
    }

    private void PickWalk()
    {
        walking = true;
        speed = Random.Range(minSpeed, maxSpeed);
        stateTimer = Random.Range(minWalkTime, maxWalkTime);
        direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        direction.Normalize();
    }

    private void PickIdle()
    {
        walking = false;
        speed = 0f;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private void FaceDirection()
    {
        if (!faceMoveDirection || direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
