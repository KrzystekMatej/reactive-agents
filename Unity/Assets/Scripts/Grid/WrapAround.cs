using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class WrapAround : MonoBehaviour
{
    private BoxCollider2D area;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        area = GameObject.FindGameObjectWithTag("Grid").GetComponentInChildren<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if (area == null) return;

        Bounds bounds = area.bounds;

        Vector2 position = rigidBody.position;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minY = bounds.min.y;
        float maxY = bounds.max.y;

        float w = maxX - minX;
        float h = maxY - minY;

        bool changed = false;

        if (position.x < minX) { position.x += w; changed = true; }
        else if (position.x > maxX) { position.x -= w; changed = true; }

        if (position.y < minY) { position.y += h; changed = true; }
        else if (position.y > maxY) { position.y -= h; changed = true; }

        if (changed)
        {
            rigidBody.position = position;
        }
    }
}
