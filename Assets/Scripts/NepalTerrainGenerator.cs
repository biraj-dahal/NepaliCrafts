using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NepalTerrainGenerator : MonoBehaviour
{
    // block prefabs
    public GameObject woodWithAlgaePrefab;
    public GameObject grassPrefab;
    public GameObject woodWithNutsPrefab;
    public GameObject brickPrefab;
    public GameObject dirtPrefab;
    public GameObject leavesPrefab;
    public GameObject woodRoughPrefab;
    public GameObject bedrockPrefab;
    public GameObject coalPrefab;
    public GameObject cobblePrefab;
    public GameObject grassTopPrefab;
    public GameObject logPrefab;
    public GameObject plankPrefab;
    public GameObject sandPrefab;
    public GameObject stonePrefab;
    public GameObject logTopPrefab;

    // Terrain params
    public int worldWidth = 64;
    public int worldLength = 64;
    public int baseHeight = 8;
    public int mountainHeight = 15;
    public int hillHeight = 8;
    public int riverDepth = 2;
    
    public float mountainNoiseScale = 0.05f;
    public float hillNoiseScale = 0.08f;
    public float detailNoiseScale = 0.2f;
    public float riverNoiseScale = 0.05f;
    public float treeNoiseScale = 0.1f;
    public float villageNoiseScale = 0.03f;
    
    public int treeChance = 3;
    public int maxTreeHeight = 4;
    
    public int terrainSeed = 42;
    public int decorationSeed = 123;

    private Dictionary<Vector3Int, GameObject> placedBlocks = new Dictionary<Vector3Int, GameObject>();
    private Transform blockParent;

    private System.Random rand;

    private void Start()
    {
        Random.InitState(terrainSeed);
        rand = new System.Random(decorationSeed);
        
        blockParent = new GameObject("Terrain Blocks").transform;
        
    
        StartCoroutine(GenerateFullTerrainCoroutine());
    }

    private IEnumerator GenerateFullTerrainCoroutine()
    {
        Debug.Log("Starting terrain generation with size: " + worldWidth + "x" + worldLength);
        
        int[,] heightMap = new int[worldWidth, worldLength];
         
     
        for (int x = 0; x < worldWidth; x++)
        {
            for (int z = 0; z < worldLength; z++)
            {
                float height = baseHeight;
                
                float mountainNoise = Mathf.PerlinNoise((x + 1000) * mountainNoiseScale, (z + 1000) * mountainNoiseScale);
                height += mountainNoise * mountainNoise * mountainHeight;
                
                float hillNoise = Mathf.PerlinNoise((x + 500) * hillNoiseScale, (z + 500) * hillNoiseScale);
                height += (hillNoise - 0.5f) * hillHeight;
                
                float detailNoise = Mathf.PerlinNoise(x * detailNoiseScale, z * detailNoiseScale);
                height += (detailNoise - 0.5f) * 2;
                
                float riverNoise = Mathf.PerlinNoise((x + 300) * riverNoiseScale, (z + 300) * riverNoiseScale);
                if (riverNoise > 0.48f && riverNoise < 0.52f)
                {
                    height -= riverDepth;
                }
                
                if (height < mountainHeight / 2 && height > baseHeight && hillNoise > 0.4f)
                {
                    height = Mathf.Floor(height / 3) * 3;
                }

                heightMap[x, z] = Mathf.FloorToInt(height);
            }
        }
        
        Debug.Log("Height map generated. Building terrain...");
        yield return null;
        
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int z = 0; z < worldLength; z++)
            {
                int height = heightMap[x, z];
                
                for (int y = 0; y <= height; y++)
                {
                    GameObject prefabToUse = GetBlockPrefabForHeight(y, height);
                    if (prefabToUse != null)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        PlaceBlock(position, prefabToUse);
                    }
                }
            }
            
            
            if (x % 5 == 0)
            {
                yield return null;
            }
        }
        
        Debug.Log("Base terrain built. Adding trees...");
        yield return null;
        
        // tree
        for (int x = 3; x < worldWidth - 3; x += 2)
        {
            for (int z = 3; z < worldLength - 3; z += 2)
            {
                int height = heightMap[x, z];
                
                if (height >= baseHeight && height < baseHeight + 15)
                {
                    float treeNoise = Mathf.PerlinNoise((x + 200) * treeNoiseScale, (z + 200) * treeNoiseScale);
                    
                    if (treeNoise > 0.6f && rand.Next(100) < treeChance)
                    {
                        GenerateTree(x, height, z);
                    }
                }
            }
            
            if (x % 10 == 0)
            {
                yield return null;
            }
        }
        
        Debug.Log("Trees added. Adding villages...");
        yield return null;
        
        for (int x = 8; x < worldWidth - 8; x += 10)
        {
            for (int z = 8; z < worldLength - 8; z += 10)
            {
                float villageNoise = Mathf.PerlinNoise((x + 500) * villageNoiseScale, (z + 500) * villageNoiseScale);
                
                if (villageNoise > 0.7f && IsRelativelyFlat(heightMap, x, z, 4))
                {
                    int height = heightMap[x, z];
                    
                    if (height >= baseHeight && height < baseHeight + 12)
                    {
                        GenerateTraditionalHouse(x, height, z);
                    }
                }
            }
        }
        
        Debug.Log("Terrain generation complete!");
    }

    private GameObject GetBlockPrefabForHeight(int y, int height)
    {
        if (y == height)
        {
            if (height < baseHeight - 1)
            {
                return sandPrefab;
            }
            else if (height > baseHeight + mountainHeight / 2)
            {
                return stonePrefab;
            }
            else if (height % 3 == 0 && height > baseHeight && height < baseHeight + 15)
            {
                return plankPrefab;
            }
            else
            {
                return grassTopPrefab;
            }
        }
        else if (y > height - 3 && y < height)
        {
            if (height < baseHeight)
            {
                return sandPrefab;
            }
            else if (height > baseHeight + mountainHeight / 2)
            {
                return stonePrefab;
            }
            else
            {
                return dirtPrefab;
            }
        }
        else if (y == 0)
        {
            return bedrockPrefab;
        }
        else
        {
            return stonePrefab;
        }
    }

    private bool IsRelativelyFlat(int[,] heightMap, int centerX, int centerZ, int radius)
    {
        int baseHeight = heightMap[centerX, centerZ];
        
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int z = centerZ - radius; z <= centerZ + radius; z++)
            {
                if (x >= 0 && x < heightMap.GetLength(0) && z >= 0 && z < heightMap.GetLength(1))
                {
                    if (Mathf.Abs(heightMap[x, z] - baseHeight) > 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void GenerateTraditionalHouse(int x, int y, int z)
    {
        int houseWidth = 3;
        int houseLength = 3;
        int houseHeight = 2;

        for (int xOffset = 0; xOffset < houseWidth; xOffset++)
        {
            for (int zOffset = 0; zOffset < houseLength; zOffset++)
            {
                PlaceBlock(new Vector3Int(x + xOffset, y, z + zOffset), plankPrefab);
                PlaceBlock(new Vector3Int(x + xOffset, y + 1, z + zOffset), plankPrefab);
            }
        }

        for (int yOffset = 0; yOffset < houseHeight; yOffset++)
        {
            for (int xOffset = 0; xOffset < houseWidth; xOffset++)
            {
                for (int zOffset = 0; zOffset < houseLength; zOffset++)
                {
                    if (xOffset > 0 && xOffset < houseWidth - 1 &&
                        zOffset > 0 && zOffset < houseLength - 1 &&
                        yOffset > 0)
                    {
                        continue;
                    }

                    if (yOffset > 0)
                    {
                        GameObject wallMaterial = woodRoughPrefab;

                        if (!(xOffset == 1 && zOffset == 0 && yOffset < 2))
                        {
                            PlaceBlock(new Vector3Int(x + xOffset, y + 1 + yOffset, z + zOffset), wallMaterial);
                        }
                    }
                }
            }
        }

        for (int xOffset = -1; xOffset <= houseWidth; xOffset++)
        {
            for (int zOffset = -1; zOffset <= houseLength; zOffset++)
            {
                PlaceBlock(new Vector3Int(x + xOffset, y + 1 + houseHeight, z + zOffset), plankPrefab);
            }
        }
    }

    private void GenerateTree(int x, int y, int z)
    {
        int treeHeight = 2 + rand.Next(maxTreeHeight - 1);

        for (int i = 0; i < treeHeight; i++)
        {
            PlaceBlock(new Vector3Int(x, y + 1 + i, z), logPrefab);
        }

        PlaceBlock(new Vector3Int(x, y + 1 + treeHeight, z), logTopPrefab);

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                for (int yOffset = 0; yOffset <= 1; yOffset++)
                {
                    if (xOffset == 0 && zOffset == 0 && yOffset < 1)
                        continue;

                    Vector3Int leafPos = new Vector3Int(
                        x + xOffset,
                        y + treeHeight - 1 + yOffset,
                        z + zOffset
                    );

                    PlaceBlock(leafPos, leavesPrefab);
                }
            }
        }
    }

    private void PlaceBlock(Vector3Int position, GameObject prefab)
    {
        if (placedBlocks.ContainsKey(position))
        {
            if (placedBlocks[position] != null)
            {
                DestroyImmediate(placedBlocks[position]);
            }
            placedBlocks.Remove(position);
        }
        
        GameObject newBlock = Instantiate(prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
        newBlock.transform.parent = blockParent;
        placedBlocks[position] = newBlock;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(worldWidth / 2, baseHeight / 2, worldLength / 2), new Vector3(worldWidth, baseHeight, worldLength));
    }
}