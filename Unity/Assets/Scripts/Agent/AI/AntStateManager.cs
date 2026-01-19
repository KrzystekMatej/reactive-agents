using UnityEngine;

public enum AntState
{
    Searching,
    Returning,
}

public sealed class AntStateManager : MonoBehaviour, AgentComponent
{
    public AntState CurrentState { get; private set; } = AntState.Searching;

    public float InternalClock { get; private set; }
    public float TimeSinceStateChange { get; private set; }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        InternalClock += dt;
        TimeSinceStateChange += dt;
    }

    public void ResetInternalClock()
    {
        InternalClock = 0f;
    }

    private void SwitchState(AntState next)
    {
        if (CurrentState == next) return;

        CurrentState = next;
        TimeSinceStateChange = 0f;
        ResetInternalClock();
    }

    public PheromoneField.Channel GetSampleChannel()
    {
        return CurrentState == AntState.Searching ? PheromoneField.Channel.ToFood : PheromoneField.Channel.ToHome;
    }

    public PheromoneField.Channel GetDepositChannel()
    {
        return CurrentState == AntState.Searching ? PheromoneField.Channel.ToHome : PheromoneField.Channel.ToFood;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.enabled) return;

        if (CurrentState == AntState.Searching)
        {
            FoodManager foodManager = collider.GetComponent<FoodManager>();
            if (foodManager != null && foodManager.HasFood())
            {
                foodManager.TakeFood();
                if (!foodManager.HasFood()) collider.enabled = false;

                SwitchState(AntState.Returning);
            }
        }
        else
        {
            AntColony colony = collider.GetComponent<AntColony>();
            if (colony != null)
            {
                colony.AddFood();
                SwitchState(AntState.Searching);
            }
        }
    }
}
