using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int worldSize = 10;
    public int chunkSize = 10;
    public float terrainHeightScale = 5f;
    public float groundLayerScale = 0.1f;
    public float mountainLayerScale = 0.02f;
    public float additionalLayerScale = 0.03f; // Adjust this for the new layer
    public float additionalLayerHeightScale = 2f; // Adjust this for the new layer
    public Material defaultMaterial;

    private void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        // Clear existing chunks
        ClearWorld();

        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                GenerateChunk(x * chunkSize, z * chunkSize);
            }
        }
    }

    void ClearWorld()
    {
        // Clear existing chunks
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateChunk(int startX, int startZ)
    {
        GameObject chunk = new GameObject("Chunk");
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        float[,] heights = new float[chunkSize + 1, chunkSize + 1];

        AssignHeights(heights, startX, startZ);

        for (int x = 0; x <= chunkSize; x++)
        {
            for (int z = 0; z <= chunkSize; z++)
            {
                vertices[x * (chunkSize + 1) + z] = new Vector3(startX + x, heights[x, z], startZ + z);
            }
        }

        int triangleIndex = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                triangles[triangleIndex] = x * (chunkSize + 1) + z;
                triangles[triangleIndex + 1] = triangles[triangleIndex + 4] = (x + 1) * (chunkSize + 1) + z;
                triangles[triangleIndex + 2] = triangles[triangleIndex + 3] = x * (chunkSize + 1) + z + 1;
                triangles[triangleIndex + 5] = (x + 1) * (chunkSize + 1) + z + 1;

                triangleIndex += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Invert the normals to flip the terrain
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        // Invert the triangles to fix backface culling
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = temp;
        }

        mesh.triangles = triangles;

        meshRenderer.material = defaultMaterial; // You can assign a material here if needed
    }

    void AssignHeights(float[,] heights, int startX, int startZ)
    {
        for (int x = 0; x <= chunkSize; x++)
        {
            for (int z = 0; z <= chunkSize; z++)
            {
                float groundNoise = Mathf.PerlinNoise((startX + x) * groundLayerScale, (startZ + z) * groundLayerScale);
                float mountainNoise = Mathf.PerlinNoise((startX + x) * mountainLayerScale, (startZ + z) * mountainLayerScale);

                // Add some variation to the terrain by multiplying with a factor
                float heightVariation = Mathf.PerlinNoise((startX + x) * 0.1f, (startZ + z) * 0.1f) * 2f;

                // Smoothly blend between heights
                float t = Mathf.InverseLerp(groundNoise, mountainNoise, Mathf.PerlinNoise((startX + x) * 0.05f, (startZ + z) * 0.05f));
                float blendedHeight = Mathf.Lerp(heightVariation, terrainHeightScale + heightVariation, t);

                // Apply additional layer on top of high terrain
                float additionalLayerNoise = Mathf.PerlinNoise((startX + x) * additionalLayerScale, (startZ + z) * additionalLayerScale);
                float additionalLayerHeight = additionalLayerNoise * additionalLayerHeightScale;

                heights[x, z] = Mathf.Max(blendedHeight, blendedHeight + additionalLayerHeight);
            }
        }
    }
}
