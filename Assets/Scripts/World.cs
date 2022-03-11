using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {

    public Material material;
    public BlockType[] blocktypes;
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