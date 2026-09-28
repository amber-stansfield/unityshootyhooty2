using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class turret : MonoBehaviour
{
    private bool turretReady = true;
    public GameObject spawnObject;
    public GameObject shoop;
    private bool inSight;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (turretReady && inSight)
        {
            this.transform.LookAt(player.transform, Vector3.up);
            
            //instantiates bullet gameobject and names it slap
            GameObject slap = Instantiate(shoop, spawnObject.transform.position, spawnObject.transform.rotation);
            turretReady = false;
            //shoots the bullet forward from the turret
            slap.GetComponent<Rigidbody>().AddForce(spawnObject.transform.forward * 55f, ForceMode.Impulse);
            StartCoroutine(shoot());
        }
        
    }

    public IEnumerator shoot()
    {
        yield return new WaitForSeconds(0.465f);
        /*animator.SetBool("shooting", false);*/
        yield return new WaitForSeconds(0.1f);
        turretReady = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            inSight = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            inSight = false;
        }
    }
}
