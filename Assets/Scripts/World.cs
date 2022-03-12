using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {
    public Transform player;
    public Vector3 spawnPos;
    public Material material;
    public BlockType[] blocktypes;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    // keep track of active chunks to delete unnecessary chunks
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    // last chunk that the player was known to be on
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        spawnPos = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2f,
            (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        // sets the player's last chunk to player spawn position
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
    }
    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; 
            x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x += 1)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.viewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z += 1)
            {
                CreateNewChunk(x, z);
            }
        }
        player.position = spawnPos;
    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActive = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x+=1)
        {
            for (int z = coord.z - VoxelData.viewDistanceInChunks; 
                z < coord.z + VoxelData.viewDistanceInChunks; z += 1)
            {
                if (isChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
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

    // mc world generation algorithm
    public byte GetVoxel(Vector3 pos)
    {
        if (!isVoxelInWorld(pos))
        {
            return 0;
        }
        if (pos.y < 1) return 2;
        else if (pos.y == VoxelData.ChunkHeight - 1) return 3;
        else
        {
            return 1;
        }
    }

    void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

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
        switch (faceIndex) {
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