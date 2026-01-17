using UnityEngine;

public interface SteeringController
{
    Vector2 CalculateSteering(float dt);
}