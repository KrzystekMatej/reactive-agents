using UnityEngine;

public static class LayerHelpers
{
    public const int layerCount = 32;

    public static LayerMask IndexToLayerMask(int layerIndex)
    {
        return 1 << layerIndex;
    }

    public static int LayerMaskToIndex(LayerMask layerMask)
    {
        for (int i = 0; i < layerCount; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                return i;
            }
        }
        return -1;
    }

    public static bool CheckLayer(int layerIndex, LayerMask layerMask)
    {
        return (IndexToLayerMask(layerIndex) & layerMask) != 0;
    }

    public static LayerMask GetCollisionLayerMask(int layer)
    {
        int mask = 0;

        for (int i = 0; i < layerCount; i++)
        {
            if (!Physics2D.GetIgnoreLayerCollision(layer, i)) mask |= 1 << i;
        }

        return mask;
    }
}
