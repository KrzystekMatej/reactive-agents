using UnityEngine;

public sealed class Movement : MonoBehaviour, AgentComponent
{
    [field: SerializeField] 
    public float Mass { get; private set; }
    [field: SerializeField] 
    public float MaxSpeed { get; private set; }
    [field: SerializeField] 
    public float MaxForce { get; private set; }
    [field: SerializeField] 
    public float MaxTurnRate { get; private set; }

    private SteeringController forceProvider;
    private Rigidbody2D rigidBody;

    public void Initialize(AgentContext agent)
    {
        forceProvider = agent.Get<SteeringController>();
        rigidBody = agent.Get<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector2 steeringForce = Vector2.ClampMagnitude(forceProvider.CalculateSteering(dt), MaxForce);
        Vector2 acceleration = steeringForce / Mass;

        rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity + acceleration * dt, MaxSpeed);

        if (rigidBody.linearVelocity.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(rigidBody.linearVelocity.y, rigidBody.linearVelocity.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(rigidBody.rotation, targetAngle, MaxTurnRate * dt);
            rigidBody.MoveRotation(newAngle);
        }
    }
}
