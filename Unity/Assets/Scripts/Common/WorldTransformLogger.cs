using UnityEngine;

public class WorldTransformLogger : MonoBehaviour
{
    private void Update()
    {
        Transform t = transform;

        Debug.Log(
            $"World Position: {t.position}, " +
            $"World Rotation (Euler): {t.rotation.eulerAngles}, " +
            $"World Scale: {t.lossyScale}"
        );
    }
}
