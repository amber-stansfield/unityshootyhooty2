using System.Collections;
using UnityEngine;

public class auxilliary_left_gun : MonoBehaviour
{

    public GameObject collid;
    private Animator animator;
    public GameObject guns;
    private bool shootReady = true;
    private bool reloadReady = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (collid.GetComponent<cap>().inWall)
        {

            transform.localRotation = Quaternion.Euler(0, 90, -90);
            transform.localPosition = new Vector3(-0.218999997f, 0.0170000009f, 0.340999991f);
            transform.localScale = new Vector3(2.07058001f, 2.07057953f, 2.07058001f);
        }
        else if (!collid.GetComponent<cap>().inWall && collid.GetComponent<cap>().leaveWall)
        {
            this.transform.localRotation = Quaternion.Euler(0, 90, 0);
            transform.localPosition = new Vector3(-0.157000005f, -0.348999977f, 0.619132996f);
            transform.localScale = new Vector3(3.07058001f, 3.07057953f, 3.07058001f);
            collid.GetComponent<cap>().leaveWall = false;
        }
        if (guns.gameObject.GetComponent<gun>().leftShoot && shootReady)
        {
            StartCoroutine(shooty1());
        }

        if (guns.gameObject.GetComponent<gun>().reloading && reloadReady)
        {
            /*animator.SetBool("reloading", true);*/
            StartCoroutine(reload());
        }

    }

    public IEnumerator reload()
    {
        reloadReady = false;
        animator.Play("reload");
        yield return new WaitForSeconds(1.3f);
        animator.Play("idle");
        reloadReady = true;
    }

    public IEnumerator shooty1()
    {
        /*if (guns.gameObject.GetComponent<gun>().bulletsInGun > 1)
        {*/
            shootReady = false;
            /*        guns.gameObject.GetComponent<gun>().bulletsInGun--;*/
            animator.SetFloat("Speed", guns.gameObject.GetComponent<gun>().animSpeed);
            animator.Play("shoot");
            Debug.Log("what");
            yield return new WaitForSeconds(guns.gameObject.GetComponent<gun>().shootDelay);
            Debug.Log(guns.gameObject.GetComponent<gun>().animSpeed + guns.gameObject.GetComponent<gun>().shootDelay);
            animator.SetFloat("Speed", 1);
            animator.Play("idle");
            guns.gameObject.GetComponent<gun>().leftShoot = false;
            shootReady = true;
        /*}*/
    }
}
