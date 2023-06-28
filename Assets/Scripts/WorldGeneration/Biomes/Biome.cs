using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Biome")]
public class Biome : ScriptableObject
{
    public string biomeName;
    public char id;

    public GameObject surfaceBlock; //grass / sand / podzol
    public GameObject subSurfaceBlock; //dirt / sandstone / gravel
    public GameObject stone;
    public GameObject bedrock;
}
