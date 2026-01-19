using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AntPheromoneDeposit : MonoBehaviour, AgentComponent
{
    [SerializeField] private Cooldown markerAdd = new Cooldown(0.25f);
    [SerializeField] private float spacingWorld = 0.25f;

    [SerializeField] private float markerIntensity = 8000.0f;
    [SerializeField] private float intensityDecayCoef = 0.05f;


    private Rigidbody2D rigidBody;
    private AntStateManager state;
    private PheromoneField field;

    private Vector2 lastDepositPosition;

    public void Initialize(AgentContext agent)
    {
        rigidBody = agent.GetComponent<Rigidbody2D>();
        state = agent.GetComponent<AntStateManager>();

        if (field == null)
        {
            field = Object.FindAnyObjectByType<PheromoneField>();
        }

        markerAdd.SetRandomOffset();
        lastDepositPosition = rigidBody.position;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (!markerAdd.UpdateAutoReset(dt)) return;

        Vector2 position = rigidBody.position;

        float spacingSqr = spacingWorld * spacingWorld;
        if ((position - lastDepositPosition).sqrMagnitude < spacingSqr) return;

        float intensity = markerIntensity * Mathf.Exp(-intensityDecayCoef * state.InternalClock);
        if (intensity <= 0.001f) return;

        field.DepositWorld(state.GetDepositChannel(), position, intensity);
        lastDepositPosition = position;
    }
}
