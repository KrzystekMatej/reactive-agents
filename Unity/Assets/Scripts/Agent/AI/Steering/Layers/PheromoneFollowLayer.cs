using UnityEngine;

public sealed class PheromoneFollowLayer : MonoBehaviour, SteeringLayer, AgentComponent
{
    [SerializeField] private int priority;

    [SerializeField] private float sensorDistance = 1.0f;
    [SerializeField] private float sensorAngleDeg = 30.0f;
    [SerializeField] private float weight = 1.0f;

    [SerializeField] private float activationThreshold = 0.02f;
    [SerializeField] private float persistenceSeconds = 0.2f;

    private PheromoneField field;

    private AntStateManager state;
    private Rigidbody2D rigidBody;
    private Movement movement;

    private float activeUntil;

    public int Priority => priority;

    public void Initialize(AgentContext agent)
    {
        rigidBody = agent.Get<Rigidbody2D>();
        movement = agent.Get<Movement>();
        state = agent.Get<AntStateManager>();
        field = FindAnyObjectByType<PheromoneField>();
    }

    public SteeringResult GetSteering(float dt)
    {
        if (field == null || state == null || rigidBody == null || movement == null) return SteeringResult.Inactive;

        PheromoneField.Channel followChannel =
            state.CurrentState == AntState.Searching ? PheromoneField.Channel.ToFood : PheromoneField.Channel.ToHome;

        Transform t = rigidBody.transform;

        Vector2 forward = t.right;
        float a = sensorAngleDeg * Mathf.Deg2Rad;

        Vector2 pF = rigidBody.position + forward * sensorDistance;
        Vector2 pL = rigidBody.position + Rotate(forward, +a) * sensorDistance;
        Vector2 pR = rigidBody.position + Rotate(forward, -a) * sensorDistance;

        float f = field.SampleWorld(followChannel, pF);
        float l = field.SampleWorld(followChannel, pL);
        float r = field.SampleWorld(followChannel, pR);

        float best = f;
        Vector2 bestDirection = forward;

        if (l > best) { best = l; bestDirection = (pL - rigidBody.position).normalized; }
        if (r > best) { best = r; bestDirection = (pR - rigidBody.position).normalized; }

        if (best < activationThreshold) return new SteeringResult(false, Vector2.zero);

        activeUntil = Time.time + persistenceSeconds;

        Vector2 desiredVel = bestDirection * movement.MaxSpeed;
        return SteeringResult.Active((desiredVel - rigidBody.linearVelocity) * weight);
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 forward = transform.right;
        float a = sensorAngleDeg * Mathf.Deg2Rad;

        Vector2 pF = (Vector2)transform.position + forward * sensorDistance;
        Vector2 pL = (Vector2)transform.position + Rotate(forward, +a) * sensorDistance;
        Vector2 pR = (Vector2)transform.position + Rotate(forward, -a) * sensorDistance;

        Gizmos.DrawLine(transform.position, pF);
        Gizmos.DrawLine(transform.position, pL);
        Gizmos.DrawLine(transform.position, pR);

        Gizmos.DrawSphere(pF, 0.05f);
        Gizmos.DrawSphere(pL, 0.05f);
        Gizmos.DrawSphere(pR, 0.05f);
    }
}
