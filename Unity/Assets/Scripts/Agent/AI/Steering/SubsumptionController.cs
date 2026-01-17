using UnityEngine;
using System.Collections.Generic;

public class SubsumptionController : MonoBehaviour, SteeringController, AgentComponent
{
    private List<SteeringLayer> layers = new List<SteeringLayer>();

    private void Awake()
    {
        layers = new List<SteeringLayer>(GetComponents<SteeringLayer>());
        layers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public Vector2 CalculateSteering(float dt)
    {
        foreach (var layer in layers)
        {
            SteeringResult result = layer.GetSteering(dt);
            if (result.IsActive)
            {
                return result.Steering;
            }
        }
        return Vector2.zero;
    }
}
