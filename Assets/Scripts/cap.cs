using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cap : MonoBehaviour
{
    public bool inWall;
    public bool leaveWall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null && other.tag != "Player" && other.tag != "projectile")
        {
            inWall = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() && other.tag != "player" && other.tag != "projectile")
        {
            inWall = false;
            leaveWall = true;
        }
        
    }
}
