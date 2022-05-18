using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk
{
    public ChunkCoord coord;
    GameObject chunkObj;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    int i = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();

    public Vector3 position;

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();
    World world;
    private bool _isActive;
    private bool isVoxelMapPopulated = false;
    public Chunk(ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;

    }
    public void Init()
    {
        chunkObj = new GameObject();
        meshFilter = chunkObj.AddComponent<MeshFilter>();
        meshRenderer = chunkObj.AddComponent<MeshRenderer>();

        //materials[0] = world.material;
        //materials[1] = world.transparentMaterial;
        meshRenderer.material = world.material;
        chunkObj.transform.SetParent(world.transform);
        chunkObj.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObj.name = "Chunk " + coord.x + ", " + coord.z;
        position = chunkObj.transform.position;
        PopulateVoxelMap();
    }
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        //mesh.subMeshCount = 2;
        //mesh.SetTriangles(triangles.ToArray(), 0);
        //mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    
    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod voxelMod = modifications.Dequeue();
            Vector3 pos = voxelMod.position -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = voxelMod.id;
        }
        ClearMeshData();
        CalculateLight();
        for (int y = 0; y < VoxelData.ChunkHeight; y += 1)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x += 1)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z += 1)
                {
                    if (world.blocktypes[voxelMap[x, y, z].id].isSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }
        lock (world.chunksToDraw)
        {
            world.chunksToDraw.Enqueue(this);
        }
    }

    void CalculateLight()
    {
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightRay = 1f;
                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];
                    if (thisVoxel.id > 0 && world.blocktypes[thisVoxel.id].transparency < lightRay)
                    {
                        lightRay = world.blocktypes[thisVoxel.id].transparency;
                    }
                    thisVoxel.globalLightPercent = lightRay;
                    voxelMap[x, y, z] = thisVoxel;
                }
            }
        }
    }
    
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y += 1)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x += 1)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z += 1)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
        isVoxelMapPopulated = true;
        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }
    }
    void ClearMeshData()
    {
        i = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
    }
    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObj != null)
            {
                chunkObj.SetActive(value);
            }
        }
    }

    public bool isEditable
    {
        get
        {
            if (!isVoxelMapPopulated)
            {
                return false;
            }
            else {
                return true;
            }
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0
            || z > VoxelData.ChunkWidth - 1);
    }

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);
        xCheck -= Mathf.FloorToInt(chunkObj.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObj.transform.position.z);
        
        voxelMap[xCheck, yCheck, zCheck].id = newID;
        
        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }
    }
    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int l = 0; l < 6; l++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[l];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + position));
            }
        }
    }
    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        // prevent index out of range
        if (!IsVoxelInChunk(x, y, z))
        {
            // prevent chunks from rendering inside each other
            return world.GetVoxelState(pos + position);
        }
        return voxelMap[x, y, z];
    }
    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);
        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);
        return voxelMap[xCheck, yCheck, zCheck];
    }
    void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        byte blockID = voxelMap[x, y, z].id;
        //bool isTransparent = world.blocktypes[blockID].renderNeighborFaces;
        for (int l = 0; l < 6; l += 1)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[l]);
            if (neighbor != null && world.blocktypes[neighbor.id].renderNeighborFaces)
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 3]]);
                //AddTexture(0); // 0 is the texture for stone
                AddTexture(world.blocktypes[blockID].GetTextureID(l));
                float lightLevel = neighbor.globalLightPercent;
                //int yPos = (int)pos.y + 1;
                //bool inShade = false;
                //while (yPos < VoxelData.ChunkHeight)
                //{
                //    if (voxelMap[(int)pos.x, yPos, (int)pos.z].id != 0)
                //    {
                //        inShade = true;
                //        break;
                //    }
                //    yPos++;
                //}
                //if (inShade)
                //{
                //    lightLevel = 0.5f;
                //}
                //else
                //{
                //    lightLevel = 0f;
                //}
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));

                //if (!renderNeighborFaces)
                //{
                triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                    triangles.Add(i + 2);
                    triangles.Add(i + 1);
                    triangles.Add(i + 3);
                //}
                //else
                //{
                //    transparentTriangles.Add(i);
                //    transparentTriangles.Add(i + 1);
                //    transparentTriangles.Add(i + 2);
                //    transparentTriangles.Add(i + 2);
                //    transparentTriangles.Add(i + 1);
                //    transparentTriangles.Add(i + 3);
                //}
                i += 4;
            }
        }
    }

    void AddTexture(int textureID)
    {
        // calculate correct placement based on texture ID
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;
        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));

    }
}

public class ChunkCoord
{
    public int x; // position of chunk we are drawing in the chunk map, not world space
    public int z;
    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }
    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);
        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }
    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.x == x && other.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;
    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }
    public VoxelState(byte _id)
    {
        id = _id;
        globalLightPercent = 0f;
    }
}