using HelloWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using static UnityEngine.GraphicsBuffer;


public class moveyment : NetworkBehaviour
{
    public float crouchYScale;
    private float moveSpeed = 1;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float wallRunSpeed;
    public float jumpHeight;
    public float jumpCoolDown;
    public bool jumpReady;
    public bool wallRunning;
    public string wallSide;
    public float airMult;
    public float groundDrag;
    public float playerHeight;
    float horizontalInput;
    float verticalInput;
    bool jumpInput;
    public bool grounded;
    private float startYScale;
    public GameObject camerapoos;
    public GameObject frcamera;
    public GameObject orientPos;
    public float xMin = -0.5f, xMax = 0.5f;
    public float timeValue = 0.0f;
    float defaultPosY = 0;
    public float bobSpeed = 14f;
    public float bobbingAmount = 0.05f;
    InputAction moveAction;
    Vector3 gravityScale = new Vector3(0, -9.81f, 0);
    float friction = 6f;
    public GameObject turret;

    public Transform orientation;

    [SerializeField] GameObject UIHolder;
    [SerializeField] GameObject nameHolder;

    [SerializeField] GameObject nameHolderParent;


    public float raceStartTime;
    public float completeTime;

    Vector3 moveDirection;

    Rigidbody rb;


    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    private HelloWorldManager managie;

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode SprintKey = KeyCode.LeftShift;
    public KeyCode CrouchKey = KeyCode.LeftControl;
    public KeyCode Interact = KeyCode.F;
    public KeyCode SpawnTurret = KeyCode.T;
    public KeyCode ResetPos = KeyCode.Z;
    /*public KeyCode PickUp = KeyCode.Mouse0;
    public KeyCode Drop = KeyCode.Mouse1;*/


    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air,
        wallRan
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {   
            frcamera.gameObject.SetActive(true);
            this.GetComponent<MeshRenderer>().enabled = false;
            nameHolderParent.SetActive(false);
        }
        else
        {
            frcamera.gameObject.GetComponent<AudioListener>().enabled = false;
            frcamera.gameObject.GetComponent<PostProcessLayer>().enabled = false;
            frcamera.gameObject.GetComponent<Camera>().enabled = false;
            
            UIHolder.SetActive(false);
            getPlayerNameServerRpc();
        }
    }


    //[ServerRpc]
    //void setNameTagServerRpc()
    //{
    //    nameHolder.GetComponent<TMP_Text>().text = AuthenticationService.Instance.PlayerName;
    //}

    // Start is called before the first frame update
    void Start()
    {
        //if (IsOwner)
        //{
        //    
        //}

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
        //SceneManager.UnloadSceneAsync("Menu");
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
        {   
            return;
        }
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        rb.rotation = orientPos.transform.rotation;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround | whatIsWall);
        defaultPosY = orientPos.transform.position.y;
        StateHandler();
        MyInput();
        wallcheck();

        SpeedControl();
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void getPlayerNameServerRpc()
    {
        string gamer = nameHolder.GetComponent<TMP_Text>().text;
        SetplayernameCallClientRpc(gamer);
    }

    [ClientRpc]
    public void SetplayernameCallClientRpc(string gamer)
    {
        nameHolder.GetComponent<TMP_Text>().text = gamer;
    }


    //[ServerRpc(RequireOwnership = false)]
    //void GetOwnPlayerRefServerRpc(NetworkObject gaming)
    //{  
    //    Debug.Log($"the gamer's id{NetworkManager.Singleton.LocalClientId}");
    //    Vector3 plap = NetworkManager.Singleton.ConnectedClients[Id].PlayerObject.transform.GetChild(0).transform.position;
    //    MoveNamePlateClientRpc(plap);
    //}


    //[ClientRpc]
    //void MoveNamePlateClientRpc(Vector3 plap)
    //{
        
    //    //nameHolder.GetComponent<RectTransform>().rotation = Quaternion.Euler(nameHolder.GetComponent<RectTransform>().rotation.x, nameHolder.GetComponent<RectTransform>().rotation.y + 180, nameHolder.GetComponent<RectTransform>().rotation.z);
    //}
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            nameHolderParent.transform.LookAt(NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(1).transform.position);
            //GetOwnPlayerRefServerRpc();
            return;
        }
        MovePlayer();
        
        rb.AddForce(gravityScale, ForceMode.Acceleration);
        if (rb.linearVelocity.magnitude > 0)
        {
            rb.AddForce(-rb.linearVelocity.normalized * friction);
        }
        
    }

    private void MyInput()
    {

        if (!IsOwner)
        {
            return;
        }
        
        if (Input.GetKey(ResetPos))
        {
            frcamera.transform.position = new Vector3(0,1,0);
            this.transform.position = new Vector3(0,1,0);
        }

        if (Input.GetKeyDown(jumpKey) && jumpReady && (grounded || wallRunning))
        {
            jumpReady = false;
            Jump();

            Invoke(nameof(resetJump), jumpCoolDown);
        }

        if (Input.GetKeyDown(CrouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(CrouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        if (Input.GetKey(SpawnTurret))
        {
            Vector3 screenCentre = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Ray ray = Camera.main.ScreenPointToRay(screenCentre);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                GameObject dude = Instantiate(turret, this.transform.position, Quaternion.LookRotation(hit.normal));
                dude.transform.position = hit.transform.position;
            }
        }

    }

    private void StateHandler()
    {
        if (grounded && Input.GetKey(SprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else if (wallRunning)
        {
            state = MovementState.wallRan;
            gravityScale = new Vector3(0, -3.2f, 0);
            friction = 1.5f;
            moveSpeed = wallRunSpeed;
        }
        else
        {
            state = MovementState.air;
            moveSpeed = sprintSpeed;
        }
        if (Input.GetKey(CrouchKey) && !wallRunning)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
    }


    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (!grounded)
        {

            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMult, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);

            //if the player isnt on the ground and is moving add bob to the camera
            if (moveDirection.magnitude > 0.1f)
            {
                timeValue += Time.deltaTime * bobSpeed;

                camerapoos.transform.position = new Vector3(camerapoos.transform.position.x, defaultPosY + Mathf.Sin(timeValue) * bobbingAmount, camerapoos.transform.position.z);
            }
            else if (moveDirection.magnitude <= 0.1f)
            {
                timeValue = 0;
                camerapoos.transform.position = new Vector3(camerapoos.transform.position.x, Mathf.Lerp(camerapoos.transform.position.y, defaultPosY, Time.deltaTime * bobSpeed), camerapoos.transform.position.z);
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }
    private void Jump()
    {
        if (wallRunning && Physics.Raycast(this.transform.position, this.transform.right * -1, 1f, whatIsWall))
        {
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
            rb.AddForce(transform.right * (jumpHeight * 6), ForceMode.Impulse);
        }
        else if (wallRunning && Physics.Raycast(this.transform.position, this.transform.right, 1f, whatIsWall))
        {
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
            rb.AddForce((transform.right * (jumpHeight * 6)) * -1, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
    }

    private void resetJump()
    {
        jumpReady = true;
    }



    private void wallcheck()
    {
        RaycastHit hit;
        //Physics.Raycast(this.transform.position, this.transform.right * -1, out hit, 1000f, whatIsWall);
        //Debug.DrawRay(transform.position, this.transform.right * -1, Color.green, 1000f);
        if ((Physics.Raycast(this.transform.position, (this.transform.right * -1), out hit, 1f, whatIsWall | whatIsGround) || Physics.Raycast(this.transform.position, this.transform.right, 1f, whatIsWall)) && !grounded)
        {
            if (hit.rigidbody == null)
            {
                wallSide = "right";
                
            }
            else
            {
                wallSide = "left";
            }
            wallRunning = true;
            Debug.Log(wallSide);
        }
        else
        {
            wallRunning = false;
            gravityScale = new Vector3(0, -9.81f, 0);
            friction = 6f;
        }
        /*debug.log*//*
        Physics.Raycast(this.transform.position, this.transform.right, out hit, 1000f, whatIsWall);*/
        /*Debug.DrawRay(transform.position, this.transform.right, Color.red, 1000f);*/
    }








/*    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("wall"))
        {
            rb.velocity = new Vector3(0, 0, 0);
        }

    }*/
}