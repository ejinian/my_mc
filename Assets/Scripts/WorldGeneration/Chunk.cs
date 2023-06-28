using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Thanks RedBlobGames!
public class Chunk : MonoBehaviour
{
    static float frequency = 0.008f;
    static float jitter = 1.5f;
    public static float[,] GenerateTemperatureMap(Vector3 chunkPositon, int seed){
        var genNoise = new FastNoiseLite();


        genNoise.SetSeed(seed + "temperature".GetHashCode());
        genNoise.SetFrequency(frequency/2);
        genNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        genNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        genNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        genNoise.SetCellularJitter(jitter);

        //int worldSize = 4000000;
        int chunkSize = 16;

        float[,] map = new float[chunkSize, chunkSize];

        for(int x = 0; x < chunkSize; x++){
            for(int z = 0; z < chunkSize; z++){
                float xInput = ((chunkPositon.x + x)); // start from center, 0 and 1 = world limit
                float zInput = ((chunkPositon.z + z)); // so player would need to be worldSize away from center to be at world limit

                float noiseValue = genNoise.GetNoise(xInput, zInput);

                map[x,z] = noiseValue;
            }
        }

        return map;
    }

    public static float[,] GenerateMoistureMap(Vector3 chunkPositon, int seed){
        var genNoise = new FastNoiseLite();


        genNoise.SetSeed(seed + "moisture".GetHashCode());
        genNoise.SetFrequency(frequency/2);
        genNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        genNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        genNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        genNoise.SetCellularJitter(jitter);

        //int worldSize = 4000000;
        int chunkSize = 16;

        float[,] map = new float[chunkSize, chunkSize];

        for(int x = 0; x < chunkSize; x++){
            for(int z = 0; z < chunkSize; z++){
                float xInput = ((chunkPositon.x + x)); // start from center, 0 and 1 = world limit
                float zInput = ((chunkPositon.z + z)); // so player would need to be worldSize away from center to be at world limit

                float noiseValue = genNoise.GetNoise(xInput, zInput);             

                map[x,z] = noiseValue;
            }
        }

        return map;
    }

    public static int GetBiome(float tempAtXZ, float moistAtXZ, float heightAtXZ){
        if(heightAtXZ >= 0.5f && heightAtXZ <= 1f){
            if(tempAtXZ >= -1f && tempAtXZ < 0f){
                return 5; //Polar Mountain
            }
            else if(tempAtXZ >= 0f && tempAtXZ <= 1f){
                return 4; //Warm Mountain
            }
        }

        if(tempAtXZ >= -1f && tempAtXZ < -0.6f){
            return 2;//Polar
        }
        else if(tempAtXZ >= -0.6f && tempAtXZ < -0.2f){
            return 3;// Tundra
        }
        else if(tempAtXZ >= -0.2f && tempAtXZ < 0.2f){
            return 3;// Spruce Forest
        }
        else if(tempAtXZ >= 0.2f && tempAtXZ < 0.6f){
            if(moistAtXZ >= -1f && moistAtXZ < -0.5f){
                return 0;// Cold Desert
            }
            else if(moistAtXZ >= -0.5f && moistAtXZ < 0.5f){
                return 1;// Prairie
            }
            else if(moistAtXZ >= 0.5f && moistAtXZ <= 1f){
                return 3;// Temperate Forest
            }
            else{
                Debug.Log("Temp: " + tempAtXZ + " | Moist: " + moistAtXZ);
                return 0;
            }
        }
        else if(tempAtXZ >= 0.6f && tempAtXZ <= 1f){
            if(moistAtXZ >= -1f && moistAtXZ < -0.5f){
                return 0;// Warm Desert
            }
            else if(moistAtXZ >= -0.5f && moistAtXZ < -0.25f){
                return 1;// Grassland
            }
            else if(moistAtXZ >= -0.25f && moistAtXZ < 0.25f){
                return 1;// Savanna
            }
            else if(moistAtXZ >= 0.25f && moistAtXZ < 0.5f){
                return 3;// Tropical Forest
            }
            else if(moistAtXZ >= 0.5f && moistAtXZ <= 1f){
                return 3;// Jungle
            }
            else{
                Debug.Log("Temp: " + tempAtXZ + " | Moist: " + moistAtXZ);
                return 0;
            }
        }

        else{
            Debug.Log("Temp: " + tempAtXZ + " | Moist: " + moistAtXZ);
            return 0;
        }
    }

    public static bool[,] GenerateWaterMap(Vector3 chunkPositon, int seed){
        var genNoise = new FastNoiseLite();

        genNoise.SetSeed(seed + "water".GetHashCode());
        genNoise.SetFrequency(frequency);
        genNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        genNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        genNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        genNoise.SetCellularJitter(jitter);

        //int worldSize = 4000000;
        int chunkSize = 16;

        bool[,] map = new bool[chunkSize, chunkSize];

        for(int x = 0; x < chunkSize; x++){
            for(int z = 0; z < chunkSize; z++){
                float xInput = ((chunkPositon.x + x)); // start from center, 0 and 1 = world limit
                float zInput = ((chunkPositon.z + z)); // so player would need to be worldSize away from center to be at world limit
                
                float noiseValue = genNoise.GetNoise(xInput, zInput);

                bool hasWater;
                if(noiseValue <= 0.4){
                    hasWater = true;
                }
                else{
                    hasWater = false;
                }

                map[x,z] = hasWater;
            }
        }
        
        return map;
    }

    public static float[,] GenerateHeightMap(Vector3 chunkPositon, int seed){
        var genNoise = new FastNoiseLite();

        genNoise.SetSeed(seed);
        genNoise.SetFrequency(frequency);
        genNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

        //int worldSize = 4000000;
        int chunkSize = 16;
        //int terrainHeight = 256;

        float[,] map = new float[chunkSize, chunkSize];

        for(int x = 0; x < chunkSize; x++){
            for(int z = 0; z < chunkSize; z++){
                float xInput = ((chunkPositon.x + x)); // start from center, 0 and 1 = world limit
                float zInput = ((chunkPositon.z + z)); // so player would need to be worldSize away from center to be at world limit

                float noiseValue = genNoise.GetNoise(xInput, zInput);
                map[x,z] = noiseValue;
            }
        }
        return map;
    }

    public static bool[,] GenereteTreeMap(int[,] terrainMap, int chunkSize){
        bool[,] treeMap = new bool[chunkSize,chunkSize];
        //Check which biome
        //Simple roll for tree to either be there or not be there
        //Have different weights for different biomes
        return treeMap;
    }
}
