using UnityEngine;

public enum AntState
{
    Searching,
    Returning,
}

public class AntStateManager : MonoBehaviour
{
    public AntState CurrentState { get; private set; } = AntState.Searching;


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CurrentState == AntState.Searching && collider.enabled)
        {
            FoodManager foodManager = collider.GetComponent<FoodManager>();
            if (foodManager != null && foodManager.HasFood())
            {
                foodManager.TakeFood();
                if (!foodManager.HasFood()) collider.enabled = false;
                CurrentState = AntState.Returning;
            }
        }
        else if (CurrentState == AntState.Returning && collider.enabled)
        {
            AntColony colony = collider.GetComponent<AntColony>();
            if (colony != null)
            {
                colony.AddFood();
                CurrentState = AntState.Searching;
            }
        }
    }
}
