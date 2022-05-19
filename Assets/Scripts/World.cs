using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;
    [Header("World Generation Values")]
    public BiomeAttributes[] biomes;
    
    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

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
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    private bool _inUI = false;

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    public GameObject debugScreen;

    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;
    private void Start()
    {
        //string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);
        //File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        // initialize random state
        Random.InitState(settings.seed);
        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);
        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
        SetGlobalLightValue();
        spawnPos = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f,
            (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        // sets the player's last chunk to player spawn position
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // this removes the block placing bug but does not allow for infinite chunk generation
        //if (playerChunkCoord != playerLastChunkCoord)
        //{
        //    // if the player has moved to a new chunk
        //    // add the new chunk to the active chunks list
        //    // and remove the old chunk from the active chunks list
        //    if (activeChunks.Contains(playerChunkCoord))
        //    {
        //        activeChunks.Remove(playerLastChunkCoord);
        //    }
        //    else
        //    {
        //        activeChunks.Add(playerChunkCoord);
        //    }
        //    // set the player's last chunk to the new chunk
        //    playerLastChunkCoord = playerChunkCoord;
        //}
        // generate new chunks as the player moves
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }
        
        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
            {
                chunksToDraw.Dequeue().CreateMesh();
            }
        }
        if (!settings.enableThreading)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance;
            x < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; x += 1)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance;
                z < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; z += 1)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
                chunksToCreate.Add(newChunk);
            }
        }
        player.position = spawnPos;
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;
        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if (!activeChunks.Contains(chunksToUpdate[index].coord))
                        activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }
    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }            
        }
    }
    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }
    void ApplyModifications()
    {
        applyingModifications = true;
        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoordFromVector3(v.position);
                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }
                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }
        applyingModifications = false;
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
        activeChunks.Clear();

        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x += 1)
        {
            for (int z = coord.z - settings.viewDistance;
                z < coord.z + settings.viewDistance; z += 1)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);
                if (isChunkInWorld(thisChunkCoord))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(thisChunkCoord);
                }
                for (int i = 0; i < previouslyActive.Count; i += 1)
                {
                    if (previouslyActive[i].Equals(thisChunkCoord))
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
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
        }
        return blocktypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return null;
        }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }
        return new VoxelState(GetVoxel(pos));
    }

    public bool inUI
    {
        get { return _inUI; }
        set { 
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
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

        // biome selection
        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
            
        }
        BiomeAttributes biome = biomes[strongestBiomeIndex];
        sumOfHeights /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

        byte voxelValue = 0;
        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
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
        // tree pass
        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                //voxelValue = 2;
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    //voxelValue = 8;
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight, false));
                }
            }
            if (biome.biomeName != "Desert")
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 3, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
                {
                    if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 3, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                    {
                        // big trees
                        modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, 13, 40, true));
                    }
                }
            }
        }
        return voxelValue;
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
    public bool renderNeighborFaces;
    public float transparency;
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

public class VoxelMod
{
    public Vector3 position;
    public byte id;
    // empty constructor
    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }
    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version;
    
    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;
    
    [Header("Controls")]
    [Range(0.2f, 10f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int seed;
}