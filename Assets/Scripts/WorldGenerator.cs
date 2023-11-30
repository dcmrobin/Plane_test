using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int worldSize = 10;
    public int chunkSize = 10;
    public float terrainHeightScale = 5f;
    public float groundLayerScale = 0.1f;
    public float mountainLayerScale = 0.02f;
    public float chasmScale = 0.03f;
    public float chasmHeight = 5f;
    public float mountainHeightScale = 2f;
    public Material defaultMaterial;
    public Material mountainMaterial;

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

        for (int x = 0; x <= chunkSize; x++)
        {
            for (int z = 0; z <= chunkSize; z++)
            {
                float groundHeight = CalculateTerrainHeight(startX + x, startZ + z, groundLayerScale);
                float mountainHeight = CalculateTerrainHeight(startX + x, startZ + z, mountainLayerScale, mountainHeightScale);
                float chasmDepth = CalculateTerrainHeight(startX + x, startZ + z, chasmScale, chasmHeight);

                float height = groundHeight + mountainHeight - chasmDepth;

                vertices[x * (chunkSize + 1) + z] = new Vector3(startX + x, height, startZ + z);
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

        AssignMaterials(meshRenderer, startX, startZ);
    }

    void AssignMaterials(MeshRenderer meshRenderer, int x, int z)
    {
        float groundNoise = Mathf.PerlinNoise(x * groundLayerScale * 0.1f, z * groundLayerScale * 0.1f);
        float mountainNoise = Mathf.PerlinNoise(x * mountainLayerScale * 0.1f, z * mountainLayerScale * 0.1f);

        if (groundNoise > mountainNoise)
        {
            meshRenderer.material = defaultMaterial;
        }
        else
        {
            meshRenderer.material = mountainMaterial;
        }
    }

    float CalculateTerrainHeight(int x, int z, float scale)
    {
        return Mathf.PerlinNoise(x * scale, z * scale) * terrainHeightScale;
    }

    float CalculateTerrainHeight(int x, int z, float scale, float heightScale)
    {
        return Mathf.PerlinNoise(x * scale, z * scale) * heightScale;
    }
}
