using UnityEngine;

public sealed class PheromoneField : MonoBehaviour
{
    public enum Channel
    {
        ToFood = 0,
        ToHome = 1
    }

    public enum InterpolationMode
    {
        Nearest = 0,
        Bilinear = 1
    }

    public enum DepositMode
    {
        Additive = 0,
        Max = 1
    }

    [SerializeField] private BoxCollider2D area;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    [SerializeField] private float toFoodEvaporationRate = 1f;
    [SerializeField] private float toHomeEvaporationRate = 1f;
    [SerializeField] private float minIntensity = 0.1f;

    [SerializeField] private bool ignoreOutsideArea = true;
    [SerializeField] private InterpolationMode samplingInterpolationMode = InterpolationMode.Bilinear;
    [SerializeField] private InterpolationMode depositInterpolationMode = InterpolationMode.Nearest;
    [SerializeField] private DepositMode depositMode = DepositMode.Max;

    [SerializeField] private Renderer targetRenderer;

    private float[] toFood;
    private float[] toHome;

    private Texture2D texture;
    private Color32[] pixels;

    private Vector2 origin;
    private Vector2 size;

    private float invSizeX;
    private float invSizeY;

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
            targetRenderer.sharedMaterial.mainTexture = texture;
        }

        RecomputeAreaCache();
    }

    private void OnValidate()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (toFoodEvaporationRate < 0f) toFoodEvaporationRate = 0f;
        if (toHomeEvaporationRate < 0f) toHomeEvaporationRate = 0f;
        if (minIntensity < 0f) minIntensity = 0f;

        if (area != null)
        {
            RecomputeAreaCache();
            SyncRendererToArea();
        }
    }

    public void Step(float dt)
    {
        float decayToFood = toFoodEvaporationRate * dt;
        float decayToHome = toHomeEvaporationRate * dt;
        for (int i = 0; i < toFood.Length; i++)
        {
            toFood[i] = Mathf.Max(toFood[i] - decayToFood, 0);
            toHome[i] = Mathf.Max(toHome[i] - decayToHome, 0);
        }
    }

    public void UploadTexture(float scale = 1.0f)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            float f = Mathf.Clamp01(toFood[i] * scale);
            float h = Mathf.Clamp01(toHome[i] * scale);
            pixels[i] = new Color32((byte)(f * 255), (byte)(h * 255), 0, 255);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
    }

    public float SampleWorld(Channel channel, Vector2 worldPos)
    {
        if (!TryWorldToUV(worldPos, out float u, out float v))
        {
            return 0f;
        }

        float[] field = GetField(channel);

        switch (samplingInterpolationMode)
        {
            case InterpolationMode.Nearest:
                return Mathf.Max(SampleNearest(field, u, v), minIntensity);
            case InterpolationMode.Bilinear:
                return Mathf.Max(SampleBilinear(field, u, v), minIntensity);
        }

        throw new System.Exception("Unreachable code");
    }

    public bool DepositWorld(Channel channel, Vector2 worldPos, float intensity)
    {
        if (intensity <= 0f) return false;
        if (!TryWorldToUV(worldPos, out float u, out float v)) return false;

        float[] field = GetField(channel);


        switch (depositInterpolationMode)
        {            
            case InterpolationMode.Nearest:
                DepositNearest(field, u, v, intensity);
                return true;
            case InterpolationMode.Bilinear:
                DepositBilinear(field, u, v, intensity);
                return true;
        }

        throw new System.Exception("Unreachable code");
    }

    public bool TryWorldToUV(Vector2 worldPos, out float u, out float v)
    {
        if (area != null && ignoreOutsideArea)
        {
            if (!area.bounds.Contains(worldPos))
            {
                u = 0f;
                v = 0f;
                return false;
            }
        }

        float x = (worldPos.x - origin.x) * invSizeX;
        float y = (worldPos.y - origin.y) * invSizeY;

        if (ignoreOutsideArea)
        {
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

        u = Mathf.Clamp01(x);
        v = Mathf.Clamp01(y);
        return true;
    }

    private float[] GetField(Channel channel)
    {
        return channel == Channel.ToFood ? toFood : toHome;
    }

    private float SampleNearest(float[] field, float u, float v)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(u * (width - 1)), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(v * (height - 1)), 0, height - 1);
        return field[x + y * width];
    }
    private float SampleBilinear(float[] field, float u, float v)
    {
        float gx = u * (width - 1);
        float gy = v * (height - 1);

        int x0 = Mathf.FloorToInt(gx);
        int y0 = Mathf.FloorToInt(gy);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        if (x1 >= width) x1 = width - 1;
        if (y1 >= height) y1 = height - 1;

        float tx = gx - x0;
        float ty = gy - y0;

        float a = field[x0 + y0 * width];
        float b = field[x1 + y0 * width];
        float c = field[x0 + y1 * width];
        float d = field[x1 + y1 * width];

        float ab = Mathf.Lerp(a, b, tx);
        float cd = Mathf.Lerp(c, d, tx);
        return Mathf.Lerp(ab, cd, ty);
    }

    private void DepositNearest(float[] field, float u, float v, float intensity)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(u * (width - 1)), 0, width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(v * (height - 1)), 0, height - 1);
        int idx = x + y * width;
        field[idx] = depositMode == DepositMode.Max ? Mathf.Max(field[idx], intensity) : field[idx] + intensity;
    }

    private void DepositBilinear(float[] field, float u, float v, float intensity)
    {
        float gx = u * (width - 1);
        float gy = v * (height - 1);

        int x0 = Mathf.FloorToInt(gx);
        int y0 = Mathf.FloorToInt(gy);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        if (x1 >= width) x1 = width - 1;
        if (y1 >= height) y1 = height - 1;

        float tx = gx - x0;
        float ty = gy - y0;

        float w00 = (1f - tx) * (1f - ty);
        float w10 = tx * (1f - ty);
        float w01 = (1f - tx) * ty;
        float w11 = tx * ty;

        int i00 = x0 + y0 * width;
        int i10 = x1 + y0 * width;
        int i01 = x0 + y1 * width;
        int i11 = x1 + y1 * width;

        float v00 = intensity * w00;
        float v10 = intensity * w10;
        float v01 = intensity * w01;
        float v11 = intensity * w11;

        field[i00] = depositMode == DepositMode.Max ? Mathf.Max(field[i00], v00) : field[i00] + v00;
        field[i10] = depositMode == DepositMode.Max ? Mathf.Max(field[i10], v10) : field[i10] + v10;
        field[i01] = depositMode == DepositMode.Max ? Mathf.Max(field[i01], v01) : field[i01] + v01;
        field[i11] = depositMode == DepositMode.Max ? Mathf.Max(field[i11], v11) : field[i11] + v11;
    }

    public void SyncRendererToArea()
    {
        if (targetRenderer == null || area == null) return;

        Bounds bounds = area.bounds;
        Transform t = targetRenderer.transform;
        t.position = new Vector3(bounds.center.x, bounds.center.y, t.position.z);
        t.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
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
            Bounds bounds = area.bounds;
            origin = bounds.min;
            size = bounds.size;
        }

        invSizeX = size.x == 0f ? 0f : 1f / size.x;
        invSizeY = size.y == 0f ? 0f : 1f / size.y;
    }

    private void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
            texture = null;
        }
    }
}
