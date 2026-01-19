using UnityEngine;

[ExecuteAlways]
public class BoundaryWalls : MonoBehaviour
{
    [SerializeField] private BoxCollider2D area;
    [SerializeField] private float thickness = 1.0f;
    [SerializeField] private float margin = 0.0f;
    [SerializeField] private LayerMask wallMask;

    private const string Prefix = "__BoundaryWall__";

#if UNITY_EDITOR
    private bool rebuildQueued;
#endif

    private void OnEnable()
    {
        QueueRebuild();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        rebuildQueued = false;
#endif
    }

    private void OnValidate()
    {
        if (thickness < 0.001f) thickness = 0.001f;
        if (margin < 0f) margin = 0f;
        QueueRebuild();
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (area == null) return;

        Transform parent = area.transform;
        Cleanup(parent);

        Vector2 size = area.size;
        Vector2 center = area.offset;

        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;

        float t = thickness;
        float m = margin;

        float xWall = hx + m + t * 0.5f;
        float yWall = hy + m + t * 0.5f;

        float wH = size.x + 2f * (m + t);
        float hV = size.y + 2f * (m + t);

        CreateWall(parent, "Top",    center + new Vector2(0f,  yWall), new Vector2(wH, t));
        CreateWall(parent, "Bottom", center + new Vector2(0f, -yWall), new Vector2(wH, t));
        CreateWall(parent, "Left",   center + new Vector2(-xWall, 0f), new Vector2(t, hV));
        CreateWall(parent, "Right",  center + new Vector2( xWall, 0f), new Vector2(t, hV));
    }

    private void QueueRebuild()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Rebuild();
            return;
        }

        if (rebuildQueued) return;
        rebuildQueued = true;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            rebuildQueued = false;
            if (this == null) return;
            Rebuild();
        };
#else
        Rebuild();
#endif
    }

    private void Cleanup(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform c = parent.GetChild(i);
            if (!c.name.StartsWith(Prefix)) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(c.gameObject);
            else Destroy(c.gameObject);
#else
            Destroy(c.gameObject);
#endif
        }
    }

    private void CreateWall(Transform parent, string suffix, Vector2 localCenter, Vector2 localSize)
    {
        GameObject boxObject = new GameObject(Prefix + suffix);
        boxObject.layer = LayerHelpers.LayerMaskToIndex(wallMask);

        Transform transform = boxObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localCenter;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        BoxCollider2D boxCollider = boxObject.AddComponent<BoxCollider2D>();
        boxCollider.offset = Vector2.zero;
        boxCollider.size = localSize;
        boxCollider.isTrigger = false;
    }
}
