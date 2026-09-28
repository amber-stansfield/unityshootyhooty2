using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using Unity.Netcode;

public class gun : NetworkBehaviour
{
    public GameObject shoop;
    private float fireState;
    static private bool gunReady = true;
    static private bool reloadReady = true;
    public GameObject spawnObject;
    private Animator animator;
    public int bulletsInGun = 6;
    public GameObject beans;
    public GameObject chamber;
    public GameObject beanpos;
    private Vector3 beanStart;
    private Vector3 beanEnd;
    public TextMeshProUGUI bulletCount;
    public TextMeshProUGUI gunMode;
    private float gunrot;
    public GameObject collid;
    public GameObject sniperAttachments;
    public GameObject burstGunMiddle;
    public GameObject burstGunLeft;
    public GameObject fullAutoAttachments;
    public bool middleShoot = false;
    public bool leftShoot = false;

    public float duration = 1f;
    private float elapsedTime;
    public float shootDelay = 0.45f;
    public float animSpeed = 1f;
    public bool reloading;

    public KeyCode ShootKey = KeyCode.Mouse0;
    public KeyCode ReloadKey = KeyCode.R;
    public KeyCode ChangeMode = KeyCode.G;

    public GunState state;
    public enum GunState
    {
        Single,
        Burst,
        Full
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        state = GunState.Single;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        bulletCount.text = bulletsInGun.ToString();
        if (Input.GetKey(ShootKey) && gunReady && reloadReady && bulletsInGun > 0)
        {
            
            StartCoroutine(shoot());

        }

        if (Input.GetKeyDown(ReloadKey) && reloadReady && bulletsInGun != 6 && gunReady)
        {
            /*animator.SetBool("reloading", true);*/
            reloadReady = false;
            StartCoroutine(reload());
        }

        if (collid.GetComponent<cap>().inWall)
        {

            transform.localRotation = Quaternion.Euler(0, 90, -90);
            transform.localPosition = new Vector3(0.410033256f, 0.00389999989f, 0.357300013f);
            transform.localScale = new Vector3(2.07058001f, 2.07057953f, 2.07058001f);
            gunReady = false;
        }
        else if (!collid.GetComponent<cap>().inWall && collid.GetComponent<cap>().leaveWall)
        {
            this.transform.localRotation = Quaternion.Euler(0, 90, 0);
            transform.localPosition = new Vector3(0.410033256f, -0.348999977f, 0.619132996f);
            transform.localScale = new Vector3(3.07058001f, 3.07057953f, 3.07058001f);
            collid.GetComponent<cap>().leaveWall = false;
            gunReady = true;
        }

        if (Input.GetKeyDown(ChangeMode))
        {
            Statehandier();
        }
            
    }

    public void Statehandier()
    {
        if (reloadReady && gunReady)
        {
            if (state == GunState.Full)
            {
                state = GunState.Single;
                gunMode.text = ("Single");
                shootDelay = 0.45f;
                animSpeed = 1f;
                sniperAttachments.SetActive(true);
                burstGunLeft.SetActive(false);
                burstGunMiddle.SetActive(false);
                fullAutoAttachments.SetActive(false);

            }
            else if (state == GunState.Single)
            {
                state = GunState.Burst;
                gunMode.text = ("Burst");
                shootDelay = 0.1f;
                animSpeed = 4.2f;
                sniperAttachments.SetActive(false);
                burstGunLeft.SetActive(true);
                burstGunMiddle.SetActive(true);
                fullAutoAttachments.SetActive(false);
            }
            else if (state == GunState.Burst)
            {
                state = GunState.Full;
                gunMode.text = ("Full Auto");
                shootDelay = 0.124f;
                animSpeed = 4f;
                sniperAttachments.SetActive(false);
                burstGunLeft.SetActive(false);
                burstGunMiddle.SetActive(false);
                fullAutoAttachments.SetActive(true);
            }
        }

    }

    [ServerRpc]
    void instanceBulletSingleServerRpc()
    {
        GameObject slap = Instantiate(shoop, spawnObject.transform.position, spawnObject.transform.rotation);
        slap.GetComponent<NetworkObject>().Spawn();
        NetworkObjectReference slapRef = new NetworkObjectReference(slap);
        instanceBulletSingleClientRpc(slap);
    }

    [ClientRpc]
    void instanceBulletSingleClientRpc(NetworkObjectReference slapRef)
    {
        if (slapRef.TryGet(out NetworkObject slap))
        slap.GetComponent<Rigidbody>().AddForce(spawnObject.transform.forward * 50f, ForceMode.Impulse);
    }


    [ServerRpc]
    void instanceBulletBurstServerRpc()
    {
        GameObject slap = Instantiate(shoop, spawnObject.transform.position, spawnObject.transform.rotation);
        slap.GetComponent<NetworkObject>().Spawn();
        NetworkObjectReference slapRef = new NetworkObjectReference(slap);
        instanceBulletBurstClientRpc(slap);
    }

    [ClientRpc]
    void instanceBulletBurstClientRpc(NetworkObjectReference slapRef)
    {
        if (slapRef.TryGet(out NetworkObject slap))
            slap.GetComponent<Rigidbody>().AddForce(spawnObject.transform.forward * 35f, ForceMode.Impulse);
    }


    [ServerRpc]
    void instanceBulletFullServerRpc()
    {
        GameObject slap = Instantiate(shoop, spawnObject.transform.position, spawnObject.transform.rotation);
        slap.GetComponent<NetworkObject>().Spawn();
        NetworkObjectReference slapRef = new NetworkObjectReference(slap);
        instanceBulletFullClientRpc(slap);
    }

    [ClientRpc]
    void instanceBulletFullClientRpc(NetworkObjectReference slapRef)
    {
        if (slapRef.TryGet(out NetworkObject slap))
            slap.GetComponent<Rigidbody>().AddForce(spawnObject.transform.forward * 20f, ForceMode.Impulse);
    }


    public IEnumerator shoot()
    {
        if (state == GunState.Single && Input.GetKeyDown(ShootKey))
        {
            instanceBulletSingleServerRpc();
            gunReady = false;
            
            /*animator.SetBool("shooting", true);*/
            bulletsInGun--;
            animator.SetFloat("Speed", animSpeed);
            animator.Play("shoot");
            yield return new WaitForSeconds(shootDelay);
            animator.SetFloat("Speed", 1);
            animator.Play("idle");
            gunReady = true;

        }

        else if (state == GunState.Burst && Input.GetKeyDown(ShootKey))
        {
            gunReady = false;
            if (bulletsInGun > 0)
            {
                instanceBulletBurstServerRpc();
                //GameObject slap = Instantiate(shoop, spawnObject.transform.position, spawnObject.transform.rotation);
                //slap.GetComponent<Rigidbody>().AddForce(spawnObject.transform.forward * 35f, ForceMode.Impulse);
                /*animator.SetBool("shooting", true);*/
                bulletsInGun--;
                animator.SetFloat("Speed", animSpeed);
                animator.Play("shoot");
                Debug.Log("what");
                yield return new WaitForSeconds(shootDelay);
                
                
                if (bulletsInGun > 0)
                {
                    middleShoot = true;
                    Debug.Log(animSpeed + shootDelay);
                    instanceBulletBurstServerRpc();
                    bulletsInGun--;
                    yield return new WaitForSeconds(shootDelay);
                    if ( bulletsInGun >= 1)
                    {
                        leftShoot = true;
                        instanceBulletBurstServerRpc();
                        bulletsInGun--;
                    }
                    
                }
                animator.SetFloat("Speed", 1);
                animator.Play("idle");

            }
            gunReady = true;
        }

        else if(state == GunState.Full)
        {
            if (bulletsInGun > 0)
            {
                instanceBulletFullServerRpc();
                gunReady = false;
                /*animator.SetBool("shooting", true);*/
                bulletsInGun--;
                animator.SetFloat("Speed", animSpeed);
                animator.Play("shoot");
                yield return new WaitForSeconds(shootDelay);
                animator.SetFloat("Speed", 1);
                animator.Play("idle");

                gunReady = true;
            }
        }
    }


    [ServerRpc]
    void ReloadingServerRpc()
    {
        animator.Play("reload");
        if (IsHost)
        {
            ClientSideHostReloadClientRpc();
        }
        StartCoroutine(WaitForSeconds());
    }

    [ClientRpc]
    void ClientSideHostReloadClientRpc()
    {
        animator.Play("reload");
        StartCoroutine(WaitForSeconds());
    }
    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(1.3f);
        animator.Play("idle");
    }

    public IEnumerator reload()
    {
        ReloadingServerRpc();
        reloading = true;
        animator.Play("reload");
        yield return new WaitForSeconds(1.3f);
        animator.Play("idle");
        bulletsInGun = 6;
        reloading = false;
        reloadReady = true;
    }

/*    private void OnCollisionEnter(Collision other)
    {
        if(other.rigidbody != null)
        {
            print("wow");
            gunrot = this.transform.rotation.z + 90;
            Mathf.Clamp(gunrot, 0, 90);
            transform.localRotation = Quaternion.Euler(0, 90, -90);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        this.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }*/
}
