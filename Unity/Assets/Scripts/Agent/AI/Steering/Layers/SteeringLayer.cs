using UnityEngine;

public struct SteeringResult
{
    public bool IsActive;
    public Vector2 Steering;
    public SteeringResult(bool isActive, Vector2 steering)
    {
        IsActive = isActive;
        Steering = steering;
    }

    public static SteeringResult Inactive => new SteeringResult(false, Vector2.zero);
    public static SteeringResult Active(Vector2 steering) => new SteeringResult(true, steering);
}

public interface SteeringLayer
{
    int Priority { get; }
    SteeringResult GetSteering(float dt);
}