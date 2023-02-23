using UnityEngine;

public struct LayerValue
{
    public string Name;
    public float Value;

    public override string ToString()
    {
        return $"{Name}: {Value}";
    }
}

public static class TerrainLayerSampler
{
    private static Vector2Int GetSplatPosition(Terrain terrain, Vector3 position)
    {
        Vector3 relativePos = position - terrain.GetPosition();
        Vector2 splatPos = Vector2.zero;
        splatPos.x = Mathf.Clamp01(relativePos.x / terrain.terrainData.size.x) * terrain.terrainData.alphamapWidth;
        splatPos.y = Mathf.Clamp01(relativePos.z / terrain.terrainData.size.z) * terrain.terrainData.alphamapHeight;

        return Vector2Int.RoundToInt(splatPos);
    }

    public static LayerValue[] GetLayerValues(Terrain terrain, Vector3 position)
    {
        Vector2Int pos = GetSplatPosition(terrain, position);
        float[,,] map = terrain.terrainData.GetAlphamaps(pos.x, pos.y, 1, 1);

        int len = terrain.terrainData.terrainLayers.Length;
        LayerValue[] layerValues = new LayerValue[len];

        for (int i = 0; i < len; i++)
        {
            layerValues[i].Value = map[0, 0, i];
            layerValues[i].Name = terrain.terrainData.terrainLayers[i].name;
        }

        return layerValues;
    }
}
