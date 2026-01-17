using UnityEngine;

public class ObstacleAvoidanceLayer : MonoBehaviour, SteeringLayer, AgentComponent
{
    [SerializeField]
    private int priority;

    public int Priority => priority;

    public SteeringResult GetSteering(float dt)
    {
        return SteeringResult.Inactive;
    }
}
