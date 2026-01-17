using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AntPheromoneDeposit : MonoBehaviour
{
    [SerializeField] private AntStateManager state;
    [SerializeField] private float rateMultiplier = 1.0f;

    private PheromoneField field;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        if (state == null) state = GetComponent<AntStateManager>();
        field = FindAnyObjectByType<PheromoneField>();
    }

    private void FixedUpdate()
    {
        if (field == null || state == null) return;

        float dt = Time.fixedDeltaTime;

        if (rigidBody.linearVelocity.sqrMagnitude < 0.0001f) return;

        var channel = state.CurrentState == AntState.Searching
            ? PheromoneField.Channel.ToHome
            : PheromoneField.Channel.ToFood;

        field.DepositWorld(channel, rigidBody.position, field.DepositRate * rateMultiplier * dt);
    }
}
