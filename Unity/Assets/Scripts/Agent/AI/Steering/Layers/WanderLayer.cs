using UnityEngine;

public class WanderLayer : MonoBehaviour, SteeringLayer, AgentComponent
{
    [SerializeField]
    private float radius;
    [SerializeField]
    private float jitter;
    [SerializeField] 
    private float distance;
    [SerializeField]
    private float weight = 1.0f;
    [SerializeField]
    private int priority;
   
    private Vector2 target;
    private Rigidbody2D rigidBody;
    private Movement movement;

    public int Priority => priority;

    public void Initialize(AgentContext agent)
    {
        rigidBody = agent.Get<Rigidbody2D>();
        movement = agent.Get<Movement>();
    }

    private void Awake()
    {
        target = Random.insideUnitCircle.normalized * radius;
    }

    public SteeringResult GetSteering(float dt)
    {
        target += Random.insideUnitCircle * (jitter * dt);

        target.Normalize();
        target *= radius;
        
        Vector2 localTarget = target + new Vector2(distance, 0);
        Transform transform = rigidBody.transform;
        Vector2 desiredDirection = ((Vector2)transform.TransformPoint(localTarget) - (Vector2)transform.position).normalized;
        Vector2 desiredVelocity = desiredDirection * movement.MaxSpeed;
        return SteeringResult.Active((desiredVelocity - rigidBody.linearVelocity) * weight);
    }


    private void OnDrawGizmos()
    {
        Vector2 center = transform.position + transform.right * distance;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, radius);

        Vector2 localTarget;
        if (Application.isPlaying)
        {
            localTarget = target + new Vector2(distance, 0f);
        }
        else
        {
            localTarget = new Vector2(distance + radius, 0f);
        }

        Vector2 worldTarget = transform.TransformPoint(localTarget);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldTarget, 0.1f);
        Gizmos.DrawLine(transform.position, worldTarget);
    }
}
