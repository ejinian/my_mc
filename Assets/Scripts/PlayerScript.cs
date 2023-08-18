using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int chunkSize = 16;

    // Start is called before the first frame update
    void Start()
    {
    }

    public static void FindPlayerSpawn(Transform player){
        RaycastHit hit;

        if(Physics.Raycast(new Vector3(0,400,0), Vector3.down, out hit)){
            player.position = new Vector3(hit.transform.position.x , hit.transform.position.y + 1, hit.transform.position.z);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }
}
