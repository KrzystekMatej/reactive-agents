using UnityEngine;

public sealed class PheromoneFollowLayer : MonoBehaviour, SteeringLayer, AgentComponent
{
    [SerializeField] private int priority = 1;
    [SerializeField] private float weight = 1.0f;

    [SerializeField] private int sampleCount = 32;
    [SerializeField] private float maxSampleDistance = 10.0f;
    [SerializeField] private float sampleAngleRange = Mathf.PI * 0.5f;

    [SerializeField] private Cooldown directionUpdate = new Cooldown(0.25f);

    [SerializeField] private Vector2 libertyCoefRange = new Vector2(0.001f, 0.01f);
    [SerializeField] private float directionNoiseRange = Mathf.PI * 0.02f;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask foodMask;
    [SerializeField] private LayerMask colonyMask;

    private float libertyCoef;
    private Vector2 currentDirection;

    private PheromoneField field;

    private AntStateManager state;
    private Rigidbody2D rigidBody;
    private Movement movement;

    public int Priority => priority;

    public void Initialize(AgentContext agent)
    {
        rigidBody = agent.Get<Rigidbody2D>();
        movement = agent.Get<Movement>();
        state = agent.Get<AntStateManager>();

        field = Object.FindAnyObjectByType<PheromoneField>();

        libertyCoef = Random.Range(libertyCoefRange.x, libertyCoefRange.y);
        directionUpdate.SetRandomOffset();
    }

    private void Start()
    {
        currentDirection = movement.GetCurrentHeadingDirection();
    }

    public SteeringResult GetSteering(float dt)
    {
        if (directionUpdate.UpdateAutoReset(dt))
        {
            currentDirection = SampleDirection();

            float noiseAngle = Random.Range(-directionNoiseRange, directionNoiseRange);
            currentDirection = MathHelpers.Rotate(currentDirection, noiseAngle);
        }

        Vector2 desiredVelocity = currentDirection * movement.MaxSpeed;
        return SteeringResult.Active((desiredVelocity - rigidBody.linearVelocity) * weight);
    }

    private Vector2 SampleDirection()
    {
        float maxIntensity = 0f;
        Vector2 bestDirection = currentDirection;

        float headingAngle = movement.GetCurrentHeadingAngle();

        for (int i = 0; i < sampleCount; i++)
        {
            float delta = Random.Range(-sampleAngleRange * 0.5f, sampleAngleRange * 0.5f);
            float angle = headingAngle + delta;

            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float distance = Random.Range(0f, maxSampleDistance);

            if (Physics2D.Raycast(rigidBody.position, direction, distance, obstacleMask)) continue;

            Vector2 target = rigidBody.position + direction * distance;

            if (IsHardGoalHit(target)) return direction;

            float intensity = field.SampleWorld(state.GetSampleChannel(), target);
            if (intensity > maxIntensity)
            {
                maxIntensity = intensity;
                bestDirection = direction;
            }

            if (Random.value < libertyCoef) break;
        }

        return bestDirection;
    }

    private bool IsHardGoalHit(Vector2 samplePoint)
    {
        if (state.CurrentState == AntState.Searching)
        {
            return Physics2D.OverlapCircle(samplePoint, 0.05f, foodMask) != null;
        }

        if (state.CurrentState == AntState.Returning)
        {
            return Physics2D.OverlapCircle(samplePoint, 0.05f, colonyMask) != null;
        }

        return false;
    }
}
