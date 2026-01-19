using UnityEngine;

public static class MathHelpers
{
    public static Vector2 Rotate(Vector2 v, float a)
    {
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
}
