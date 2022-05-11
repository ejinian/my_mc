using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPos;
    public Material material;
    public Material transparentMaterial;
    
    public BlockType[] blocktypes;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    // keep track of active chunks to delete unnecessary chunks
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    // last chunk that the player was known to be on
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private bool isCreatingChunks;
    public GameObject debugScreen;
    private void Start()
    {
        // initialize random state
        Random.InitState(seed);
        spawnPos = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f,
            (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        // sets the player's last chunk to player spawn position
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        // generate new chunks as the player moves
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks;
            x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x += 1)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z += 1)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
        player.position = spawnPos;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;
        List<ChunkCoord> previouslyActive = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x += 1)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks;
                z < coord.z + VoxelData.ViewDistanceInChunks; z += 1)
            {
                if (isChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }
                for (int i = 0; i < previouslyActive.Count; i += 1)
                {
                    if (previouslyActive[i].Equals(new ChunkCoord(x, z)))
                    {
                        previouslyActive.RemoveAt(i);
                    }
                }
            }
        }
        foreach (ChunkCoord c in previouslyActive)
        {
            chunks[c.x, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        //int xCheck = Mathf.FloorToInt(_x);
        //int yCheck = Mathf.FloorToInt(_y);
        //int zCheck = Mathf.FloorToInt(_z);

        //int xChunk = xCheck / VoxelData.ChunkWidth;
        //int zChunk = zCheck / VoxelData.ChunkWidth;
        //xCheck -= (xChunk * VoxelData.ChunkWidth);
        //zCheck -= (zChunk * VoxelData.ChunkWidth);
        //return blocktypes[chunks[xChunk, zChunk].voxelMap[xCheck, yCheck, zCheck]].isSolid;
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
        {
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
        }
        return blocktypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
        {
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;
        }
        return blocktypes[GetVoxel(pos)].isTransparent;
    }    

    // mc world generation algorithm
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!isVoxelInWorld(pos))
        {
            return 0;
        }
        if (yPos == 0)
        {
            return 0; // bedrock, for now it's just stone
        }
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z),
            0, biome.terrainScale)) + biome.solidGroundHeight; // vec3 in vid
        byte voxelValue = 0;
        if (yPos == terrainHeight)
        {
            voxelValue = 3;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 6;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 1;
        }
        if (voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    { // override block based on lode configuration
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        return voxelValue;

        //if (pos.y < 1) return 2;
        //// above stone
        //else if (pos.y == VoxelData.ChunkHeight - 1) {
        //    float tempNoise = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, 0.1f);
        //    if (tempNoise < 0.5f)
        //    {
        //        return 3;
        //    }else
        //    {
        //        return 5;
        //    }
        //}
        //else
        //{
        //    return 1;
        //}
    }

    //void CreateNewChunk(int x, int z)
    //{
    //    chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
    //    activeChunks.Add(new ChunkCoord(x, z));
    //}

    bool isChunkInWorld(ChunkCoord coord)
    {
        return (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 &&
            coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1);
    }

    bool isVoxelInWorld(Vector3 pos)
    {
        return (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels);
    }
}
[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public Sprite icon;

    [Header("Texture values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;


    // Order: back front top bottom left right
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTexture");
                return 0;
        }

    }
}