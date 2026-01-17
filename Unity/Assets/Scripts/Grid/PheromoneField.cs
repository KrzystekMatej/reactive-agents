using UnityEngine;

public sealed class PheromoneField : MonoBehaviour
{
    public enum Channel
    {
        ToFood = 0,
        ToHome = 1
    }

    public enum DisplayMode
    {
        BlendRGB = 0,
        ToFoodOnly = 1,
        ToHomeOnly = 2,
        Difference = 3
    }

    public enum BoundaryMode
    {
        IgnoreOutside = 0,
        Clamp = 1,
        Wrap = 2
    }

    [SerializeField] private BoxCollider2D area;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    [SerializeField] private float evaporationPerSecond = 0.5f;
    [SerializeField] private float depositAmountPerSecond = 2.0f;
    [SerializeField] private float depositRadiusWorld = 0.3f;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private DisplayMode displayMode = DisplayMode.BlendRGB;
    [SerializeField] private BoundaryMode boundaryMode = BoundaryMode.Wrap;

    private float[] toFood;
    private float[] toHome;

    private Texture2D texture;
    private Color32[] pixels;

    private Vector2 origin;
    private Vector2 size;

    private float cellSizeX;
    private float cellSizeY;

    public float DepositRate => depositAmountPerSecond;

    private void Awake()
    {
        toFood = new float[width * height];
        toHome = new float[width * height];
        pixels = new Color32[width * height];

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = texture;
        }

        RecomputeAreaCache();
    }

    private void OnValidate()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (depositRadiusWorld < 0f) depositRadiusWorld = 0f;
        if (evaporationPerSecond < 0f) evaporationPerSecond = 0f;
        if (depositAmountPerSecond < 0f) depositAmountPerSecond = 0f;

        if (area != null)
        {
            RecomputeAreaCache();
            SyncRendererToArea();
        }
    }

    public float SampleWorld(Channel channel, Vector2 worldPos)
    {
        if (!TryWorldToGridUV(worldPos, out float u, out float v)) return 0f;
        return SampleBilinear(GetField(channel), u, v);
    }

    public void DepositWorld(Channel channel, Vector2 worldPos, float amount)
    {
        if (!TryWorldToGridUV(worldPos, out float u, out float v)) return;

        float[] field = GetField(channel);

        int cx = Mathf.FloorToInt(u * (width - 1));
        int cy = Mathf.FloorToInt(v * (height - 1));

        int rx = Mathf.CeilToInt(depositRadiusWorld / cellSizeX);
        int ry = Mathf.CeilToInt(depositRadiusWorld / cellSizeY);

        for (int y = cy - ry; y <= cy + ry; y++)
        {
            int wy = WrapIndex(y, height);
            if (wy < 0) continue;

            for (int x = cx - rx; x <= cx + rx; x++)
            {
                int wx = WrapIndex(x, width);
                if (wx < 0) continue;

                Vector2 p = GridToWorld(wx, wy);
                float d = Vector2.Distance(p, worldPos);

                if (boundaryMode == BoundaryMode.Wrap)
                {
                    d = ToroidalDistance(worldPos, p);
                }

                if (d > depositRadiusWorld) continue;

                float w = 1.0f - (d / depositRadiusWorld);
                int idx = wx + wy * width;
                field[idx] += amount * w;
            }
        }
    }

    public void Step(float dt)
    {
        float k = Mathf.Clamp01(1.0f - evaporationPerSecond * dt);
        for (int i = 0; i < toFood.Length; i++)
        {
            toFood[i] *= k;
            toHome[i] *= k;
        }
    }

    public void UploadTexture(float scale = 1.0f)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            float f = Mathf.Clamp01(toFood[i] * scale);
            float h = Mathf.Clamp01(toHome[i] * scale);

            byte r;
            byte g;
            byte b;

            switch (displayMode)
            {
                case DisplayMode.ToFoodOnly:
                    r = (byte)(f * 255);
                    g = 0;
                    b = 0;
                    break;

                case DisplayMode.ToHomeOnly:
                    r = 0;
                    g = (byte)(h * 255);
                    b = 0;
                    break;

                case DisplayMode.Difference:
                    float diff = Mathf.Clamp01((f - h) * 0.5f + 0.5f);
                    byte d = (byte)(diff * 255);
                    r = d;
                    g = d;
                    b = d;
                    break;

                default:
                    r = (byte)(f * 255);
                    g = (byte)(h * 255);
                    b = 0;
                    break;
            }

            pixels[i] = new Color32(r, g, b, 255);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
    }

    public void SyncRendererToArea()
    {
        if (targetRenderer == null || area == null) return;

        Bounds b = area.bounds;
        Transform t = targetRenderer.transform;
        t.position = new Vector3(b.center.x, b.center.y, t.position.z);
        t.localScale = new Vector3(b.size.x, b.size.y, 1f);
    }

    public void RecomputeAreaCache()
    {
        if (area == null)
        {
            origin = Vector2.zero;
            size = Vector2.one;
        }
        else
        {
            Bounds b = area.bounds;
            origin = b.min;
            size = b.size;
        }

        cellSizeX = size.x / width;
        cellSizeY = size.y / height;
    }

    private float[] GetField(Channel channel)
    {
        return channel == Channel.ToFood ? toFood : toHome;
    }

    private bool TryWorldToGridUV(Vector2 worldPos, out float u, out float v)
    {
        float x = (worldPos.x - origin.x) / size.x;
        float y = (worldPos.y - origin.y) / size.y;

        switch (boundaryMode)
        {
            case BoundaryMode.Wrap:
                u = Mathf.Repeat(x, 1f);
                v = Mathf.Repeat(y, 1f);
                return true;

            case BoundaryMode.Clamp:
                u = Mathf.Clamp01(x);
                v = Mathf.Clamp01(y);
                return true;

            default:
                if (x < 0f || x > 1f || y < 0f || y > 1f)
                {
                    u = 0f;
                    v = 0f;
                    return false;
                }
                u = x;
                v = y;
                return true;
        }
    }

    private int WrapIndex(int i, int n)
    {
        if (boundaryMode == BoundaryMode.Wrap)
        {
            int m = i % n;
            if (m < 0) m += n;
            return m;
        }

        if ((uint)i >= (uint)n) return -1;
        return i;
    }

    private float ToroidalDistance(Vector2 a, Vector2 b)
    {
        float dx = Mathf.Abs(a.x - b.x);
        float dy = Mathf.Abs(a.y - b.y);

        float w = size.x;
        float h = size.y;

        if (dx > w * 0.5f) dx = w - dx;
        if (dy > h * 0.5f) dy = h - dy;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private float SampleBilinear(float[] field, float u, float v)
    {
        float x = u * (width - 1);
        float y = v * (height - 1);

        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        int x1 = Mathf.Min(x0 + 1, width - 1);
        int y1 = Mathf.Min(y0 + 1, height - 1);

        float tx = x - x0;
        float ty = y - y0;

        float a = field[x0 + y0 * width];
        float b = field[x1 + y0 * width];
        float c = field[x0 + y1 * width];
        float d = field[x1 + y1 * width];

        float ab = Mathf.Lerp(a, b, tx);
        float cd = Mathf.Lerp(c, d, tx);
        return Mathf.Lerp(ab, cd, ty);
    }

    private Vector2 GridToWorld(int x, int y)
    {
        float wx = origin.x + (x + 0.5f) * cellSizeX;
        float wy = origin.y + (y + 0.5f) * cellSizeY;
        return new Vector2(wx, wy);
    }
}
