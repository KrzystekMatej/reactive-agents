using UnityEngine;

public sealed class PheromoneSystem : MonoBehaviour
{
    [SerializeField] private PheromoneField field;
    [SerializeField] private float simHz = 20.0f;
    [SerializeField] private float uploadHz = 10.0f;
    [SerializeField] private float visualScale = 0.2f;

    private float simAcc;
    private float uploadAcc;

    private void Update()
    {
        float dt = Time.deltaTime;

        simAcc += dt;
        float simStep = 1.0f / Mathf.Max(1.0f, simHz);
        while (simAcc >= simStep)
        {
            simAcc -= simStep;
            field.Step(simStep);
        }

        uploadAcc += dt;
        float uploadStep = 1.0f / Mathf.Max(1.0f, uploadHz);
        if (uploadAcc >= uploadStep)
        {
            uploadAcc -= uploadStep;
            field.UploadTexture(visualScale);
        }
    }
}
