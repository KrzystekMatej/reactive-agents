using UnityEngine;

public sealed class ObstacleAvoidanceLayer : MonoBehaviour, SteeringLayer, AgentComponent
{
    [SerializeField] private int priority = 2;
    [SerializeField] private float weight = 1.0f;

    [SerializeField] private float lookAhead = 5.0f;
    [SerializeField] private LayerMask obstacleMask;

    private Rigidbody2D rigidBody;
    private Movement movement;

    public int Priority => priority;

    public void Initialize(AgentContext agent)
    {
        rigidBody = agent.Get<Rigidbody2D>();
        movement = agent.Get<Movement>();
    }

    public SteeringResult GetSteering(float dt)
    {
        Vector2 heading = movement.GetCurrentHeadingDirection();
        RaycastHit2D hit = Physics2D.Raycast(rigidBody.position, heading, lookAhead, obstacleMask);

        if (!hit) return SteeringResult.Inactive;

        Vector2 desiredDirection = Vector2.Reflect(heading, hit.normal).normalized;
        Vector2 desiredVelocity = desiredDirection * movement.MaxSpeed;
        return SteeringResult.Active((desiredVelocity - rigidBody.linearVelocity) * weight);
    }

    private void OnDrawGizmos()
    {
        if (rigidBody == null || movement == null) return;
        Vector2 heading = movement.GetCurrentHeadingDirection();
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rigidBody.position, rigidBody.position + heading * lookAhead);
    }
}
