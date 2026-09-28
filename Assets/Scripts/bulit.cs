using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;


public class bulit : MonoBehaviour
{
    public Sprite bulletDecal;
    public GameObject slapp;
    public Vector3 normie;
    private GameObject woo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnCollisionEnter(Collision other)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit) && hit.rigidbody != null && hit.rigidbody.tag != "Player")
        {
            woo = Instantiate(slapp, (hit.point += hit.normal *0.01f), Quaternion.LookRotation(hit.normal));
            woo.transform.parent = hit.transform;
        }
    }
}
