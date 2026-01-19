using UnityEngine;

[System.Serializable]
public struct Cooldown
{
    [SerializeField] private float target;
    [SerializeField] private float value;

    public float Target => target;
    public float Value => value;

    public Cooldown(float targetSeconds, float initialValueSeconds = 0f)
    {
        target = Mathf.Max(0f, targetSeconds);
        value = Mathf.Max(0f, initialValueSeconds);
    }

    public void SetTarget(float targetSeconds)
    {
        target = Mathf.Max(0f, targetSeconds);
        if (value > target) value = target;
    }

    public void SetRandomOffset()
    {
        value = (target <= 0f) ? 0f : Random.value * target;
    }

    public void Update(float dt)
    {
        value += dt;
    }

    public bool Ready()
    {
        return value >= target;
    }

    public bool UpdateAutoReset(float dt)
    {
        value += dt;
        if (value >= target)
        {
            value -= target;
            return true;
        }
        return false;
    }

    public float Ratio()
    {
        if (target <= 0f) return 1f;
        return Mathf.Clamp01(value / target);
    }
}