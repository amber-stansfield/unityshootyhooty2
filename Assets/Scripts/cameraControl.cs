using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class cameraControl : NetworkBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform cameraPosition;
    public GameObject shmoove;
/*    public menuscrip waow;*/

    float xRotation;
    float yRotation;


    void Start()
    {
        transform.position = cameraPosition.position;
        Cursor.lockState = CursorLockMode.Locked;
        QualitySettings.vSyncCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
      
        if (!IsOwner)
        {
            return;
        }
        /*if (waow.control == true)*/
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation,transform.localEulerAngles.z);

        if (!shmoove.GetComponent<moveyment>().wallRunning)
        {
            plap(0f);
            /*transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f), 10f * Time.deltaTime);*/
        }      

        if (shmoove.GetComponent<moveyment>().wallRunning && shmoove.GetComponent<moveyment>().wallSide == "left")
        {
            Debug.Log("poop2 the poopening");
            plap(-15f);
            /*transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -15f), 20f * Time.deltaTime);*/
        }
        else if(shmoove.GetComponent<moveyment>().wallRunning && shmoove.GetComponent<moveyment>().wallSide == "right")
        {
            Debug.Log("poop");
            plap(15f);
            /*transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 15f), 20f * Time.deltaTime);*/
        }

        transform.position = Vector3.Lerp(transform.position,new Vector3(transform.position.x,cameraPosition.position.y,transform.position.z),20f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, new Vector3(cameraPosition.position.x,transform.position.y,cameraPosition.position.z), 10f * Time.deltaTime);
        /*transform.position = cameraPosition.position;*/
            
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        
    }

    void plap(float targetAngle)
    {
        float currentAngle = transform.localEulerAngles.z;
        //keep within 0 - 360
        if (currentAngle > 180) currentAngle -= 360;

        float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
        float newAngle = Mathf.Lerp(currentAngle, currentAngle + deltaAngle, Time.deltaTime * 20f);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, newAngle);
    }
}
