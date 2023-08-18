using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;

public class World : MonoBehaviour
{
    public GameObject player; // who is the player
    Vector3 lastPlayerChunk;
    bool playerSet = false;

    public GameObject biomeManager; //who is the biomeManager
    BiomeManager biomeScript;

    public int viewDistance;
    public int chunkSize;
    float[,] heightMap;
    float[,] temperatureMap;
    float[,] moistureMap;
    bool[,] waterMap;

    public string seedString; //our seed for the world
    int seed;

    // Start is called before the first frame update
    void Start()
    {
        seed = seedString.GetHashCode();
        biomeScript = biomeManager.GetComponent<BiomeManager>();
        GenerateWorld();

    }

    void GenerateWorld(){
        Vector3 currentPlayerChunk = new Vector3((int)(player.transform.position.x/chunkSize), 0 , (int)(player.transform.position.z/chunkSize));
        if(!playerSet){
            lastPlayerChunk = currentPlayerChunk;
            
            for(int chunkX = (int)lastPlayerChunk.x - viewDistance; chunkX <= (int)lastPlayerChunk.x + viewDistance; chunkX++){
                for(int chunkZ = (int)lastPlayerChunk.z - viewDistance; chunkZ <= (int)lastPlayerChunk.z + viewDistance; chunkZ++){
                    
                    //chunkObject.SetActive(false);
                    StartCoroutine(GenerateChunk(chunkX, chunkZ));
                }
            }
            PlayerScript.FindPlayerSpawn(player.transform);
            playerSet = true;
        }
        else if(lastPlayerChunk != currentPlayerChunk){
            lastPlayerChunk = currentPlayerChunk;
            for(int chunkX = (int)lastPlayerChunk.x - viewDistance; chunkX <= (int)lastPlayerChunk.x + viewDistance; chunkX++){
                for(int chunkZ = (int)lastPlayerChunk.z - viewDistance; chunkZ <= (int)lastPlayerChunk.z + viewDistance; chunkZ++){
                    
                    //chunkObject.SetActive(false);
                    StartCoroutine(GenerateChunk(chunkX, chunkZ));
                }
            }
        }
    }

    IEnumerator GenerateChunk(int chunkX, int chunkZ){
        if(GameObject.Find("World/Chunk(" + chunkX + ", " + chunkZ + ")")){
            GameObject.Find("World/Chunk(" + chunkX + ", " + chunkZ + ")").SetActive(true);
            yield break;
        }
        GameObject chunkObject = new GameObject("Chunk(" + chunkX + ", " + chunkZ + ")");
        chunkObject.transform.SetPositionAndRotation(new Vector3((chunkX * chunkSize), 0, (chunkZ * chunkSize)), Quaternion.identity);
        chunkObject.transform.SetParent(transform);
        var flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic;
        GameObjectUtility.SetStaticEditorFlags(chunkObject, flags);

        

        Vector3 chunkPosition = new Vector3(chunkObject.transform.position.x, chunkObject.transform.position.y, chunkObject.transform.position.z);
        temperatureMap = Chunk.GenerateTemperatureMap(chunkPosition, seed);
        moistureMap = Chunk.GenerateMoistureMap(chunkPosition, seed);
        //waterMap = Chunk.GenerateWaterMap(chunkPosition, seed);
        heightMap = Chunk.GenerateHeightMap(chunkPosition, seed);
        
        for(int blockX = 0; blockX < chunkSize; blockX++){
            for(int blockZ = 0; blockZ < chunkSize; blockZ++){
                
                int currentBiome = Chunk.GetBiome(temperatureMap[blockX,blockZ], moistureMap[blockX,blockZ], heightMap[blockX, blockZ]);

                int height = 64;
                int minHeight = 64;
                int maxHeight = 90;
                height = (int)Mathf.Round((heightMap[blockX,blockZ]*(maxHeight-minHeight))+minHeight);

                for(int blockY = 0; blockY <= height; blockY++){
                    //Debug.Log(biomeMap[blockX,blockZ]);
                    if(blockY >= height-3){
                        GameObject blockObject = Instantiate(biomeScript.biomes[currentBiome].surfaceBlock, new Vector3(blockX+chunkPosition.x, blockY+chunkPosition.y, blockZ+chunkPosition.z), Quaternion.identity, chunkObject.transform);
                        blockObject.transform.name = biomeScript.biomes[currentBiome].surfaceBlock.transform.name + "[" + blockX + ", " + blockY + ", " + blockZ + "]";
                    }
                    else if(blockY < height-3 && blockY >= height-6){
                        GameObject blockObject = Instantiate(biomeScript.biomes[currentBiome].subSurfaceBlock, new Vector3(blockX+chunkPosition.x, blockY+chunkPosition.y, blockZ+chunkPosition.z), Quaternion.identity, chunkObject.transform);
                        blockObject.transform.name = biomeScript.biomes[currentBiome].subSurfaceBlock.transform.name + "[" + blockX + ", " + blockY + ", " + blockZ + "]";                                            
                    }else if(blockY == 0){
                        GameObject blockObject = Instantiate(biomeScript.biomes[currentBiome].bedrock, new Vector3(blockX+chunkPosition.x, blockY+chunkPosition.y, blockZ+chunkPosition.z), Quaternion.identity, chunkObject.transform);
                        blockObject.transform.name = biomeScript.biomes[currentBiome].bedrock.transform.name + "[" + blockX + ", " + blockY + ", " + blockZ + "]";                                            
                    }
                    else{
                        GameObject blockObject = Instantiate(biomeScript.biomes[currentBiome].stone, new Vector3(blockX+chunkPosition.x, blockY+chunkPosition.y, blockZ+chunkPosition.z), Quaternion.identity, chunkObject.transform);
                        blockObject.transform.name = biomeScript.biomes[currentBiome].stone.transform.name + "[" + blockX + ", " + blockY + ", " + blockZ + "]";                                                
                    }
                }
            }
                    yield return null;
        }
    }

    void FixedUpdate()
    {
        GenerateWorld();
    }
}
