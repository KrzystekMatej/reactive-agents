using UnityEngine;

public sealed class Movement : MonoBehaviour, AgentComponent
{
    [field: SerializeField]
    public float Mass { get; private set; } = 1f;
    [field: SerializeField]
    public float MaxSpeed { get; private set; } = 40f;
    [field: SerializeField]
    public float MaxForce { get; private set; } = 100f;
    [field: SerializeField]
    public float MaxTurnRate { get; private set; } = 360f;

    private SteeringController forceProvider;
    private Rigidbody2D rigidBody;

    public const float MinHeadingSpeed = 0.02f;
    public const float MinHeadingSpeedSqr = MinHeadingSpeed * MinHeadingSpeed;

    public void Initialize(AgentContext agent)
    {
        forceProvider = agent.Get<SteeringController>();
        rigidBody = agent.Get<Rigidbody2D>();
        rigidBody.rotation = Random.Range(0f, 360f);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector2 steeringForce = Vector2.ClampMagnitude(forceProvider.CalculateSteering(dt), MaxForce);
        Vector2 acceleration = steeringForce / Mass;

        rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity + acceleration * dt, MaxSpeed);

        if (rigidBody.linearVelocity.sqrMagnitude > MinHeadingSpeedSqr)
        {
            float targetAngle = Mathf.Atan2(rigidBody.linearVelocity.y, rigidBody.linearVelocity.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(rigidBody.rotation, targetAngle, MaxTurnRate * dt);
            rigidBody.MoveRotation(newAngle);
        }
    }

    public float GetCurrentHeadingAngle()
    {
        if (rigidBody.linearVelocity.sqrMagnitude > MinHeadingSpeedSqr) 
            return Mathf.Atan2(rigidBody.linearVelocity.y, rigidBody.linearVelocity.x);

        return rigidBody.rotation * Mathf.Deg2Rad;
    }

    public Vector2 GetCurrentHeadingDirection()
    {
        if (rigidBody.linearVelocity.sqrMagnitude > MinHeadingSpeedSqr) 
            return rigidBody.linearVelocity.normalized;

        float angle = rigidBody.rotation * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
