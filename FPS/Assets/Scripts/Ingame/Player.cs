using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Photon.MonoBehaviour {

    [Header ("Networking")]
    public bool isLocal = false; //Is playing locally
    public float networkLerpSmoothing = 10; //Smoothing of network player
    Vector3 networkPosition; //Position of network player
    Quaternion networkRotation; //Rotation of network player

    [Header ("Movement")]
    public float mainSpeed = 15; //Speed of the player
    public float sprintMultiplier = 1.3f; //Multiplication of speed when sprinting
    public float crouchMultiplier = 0.75f; //Multiplication of speed when crouching
    public float inAirMultiplier = 0.2f; //Multiplication of speed when in-air
    bool isGrounded; //Check if grounded
    Vector3 movementVector; //Movement vector of player

    [Space (10)]
    public float jumpForce = 250; //Force of the jump
    public Vector3 jumpBoxCheckSize; //Size of the box that detects jumpable layers
    public LayerMask playerLayer; //Layer of the player
    Rigidbody playerRigidbody; //Rigidbody of the player

    [Header ("Scene")]
    public string sceneCameraTag = "SceneCamera"; //Tag of the scene camera
    GameObject sceneCam; //Camera of the scene

    [Header ("Camera")]
    public GameObject playerCam; //Camera of the player
    public int sensitivity = 100; //Sensitivity of the camera
    public Vector2 clampRotation; //Min and max clamp
    float camRotation = 0; //Rotation of camera on the X axis.
    bool camToggle; //Toggle camera bool

    void Start () {

        //Set scene camera
        sceneCam = GameObject.FindGameObjectWithTag (sceneCameraTag);

        //Set player rigidbody
        playerRigidbody = GetComponent<Rigidbody> ();

        if (!isLocal)
            if (photonView.isMine) {
                print ("Photon View is mine");
                ChangeCam (true);
            }
        else {
            print ("Photon View isn't mine");
            StartCoroutine (LerpPlayer ());
        } else {
            print ("Is Local");
            ChangeCam (true);
        }

    }

    void ChangeCam (bool changeToPlayerCam) {

        //Change camera
        if (changeToPlayerCam) {

            //From scene to player
            sceneCam.SetActive (false);
            playerCam.SetActive (true);
        } else {

            //From player to scene
            sceneCam.SetActive (true);
            playerCam.SetActive (false);
        }
    }

    void Update () {
        if (photonView.isMine) {
            RotatePlayer ();
            RotateCameraFirstPerson ();
            Jumper ();

            //TEMPORARY
            if (Input.GetButtonDown ("Temp")) {
                ChangeCam (!camToggle);
                camToggle = !camToggle;
            }
        }

    }

    void FixedUpdate () {
        if (photonView.isMine) {
            Move ();
        }
    }

    void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext (transform.position); //Send player position
            stream.SendNext (transform.rotation); //Send player rotation
        } else {
            networkPosition = (Vector3) stream.ReceiveNext (); //Receive player position
            networkRotation = (Quaternion) stream.ReceiveNext (); //Receive player rotation
        }

    }

    public IEnumerator LerpPlayer () {
        while (true) {
            if (!isLocal) {
                transform.position = Vector3.Lerp (transform.position, networkPosition, networkLerpSmoothing * Time.deltaTime);
                transform.rotation = Quaternion.Lerp (transform.rotation, networkRotation, networkLerpSmoothing * Time.deltaTime);
            }
            yield return null;
        }
    }

    public void Jumper () {

        //Set layermask
        LayerMask notPlayer = ~playerLayer;

        Vector3 halfExtends = jumpBoxCheckSize / 2f;

        //Check if the player is grounded
        if (Physics.CheckBox (transform.position, halfExtends, transform.rotation, notPlayer)) {

            //Set isGrounded bool true
            isGrounded = true;

            //Check if player presses the Jump button
            if (Input.GetButtonDown ("Jump")) {

                //Add force to the player rigidbody
                playerRigidbody.AddRelativeForce (Vector3.up * jumpForce + movementVector * jumpForce, ForceMode.Impulse);
            }
        } else {

            //Set isGrounded bool false
            isGrounded = false;
        }
    }

    #region MoveAndRotate
    void Move () {

        //Vector to translate to position

        movementVector.x = Input.GetAxis ("Horizontal");
        movementVector.y = 0;
        movementVector.z = Input.GetAxis ("Vertical");

        //Check multipliers
        if (!isGrounded)
            movementVector *= inAirMultiplier;
        else if (Input.GetButton ("Sprint"))
            movementVector *= sprintMultiplier;
        else if (Input.GetButton ("Crouch"))
            movementVector *= crouchMultiplier;

        Vector3 final = movementVector * mainSpeed * Time.deltaTime;

        //Translate the position
        transform.Translate (final);

    }

    void RotatePlayer () {

        //Rotate the player
        transform.Rotate (Vector3.up * Input.GetAxis ("Mouse X") * sensitivity * Time.deltaTime);
    }

    void RotateCameraFirstPerson () {

        //Set new X axis rotation
        camRotation += -1 * Input.GetAxis ("Mouse Y") * Time.deltaTime * sensitivity;
        camRotation = Clamp (camRotation, clampRotation.x, clampRotation.y);

        //Rotate cam on X axis
        Vector3 cameraRotation = playerCam.transform.localRotation.eulerAngles;
        cameraRotation.x = camRotation;
        playerCam.transform.localRotation = Quaternion.Euler (cameraRotation);
    }

    float Clamp (float input, float min, float max) {
        if (input < min)
            return min;
        else if (input > max)
            return max;
        else
            return input;
    }
    #endregion

}