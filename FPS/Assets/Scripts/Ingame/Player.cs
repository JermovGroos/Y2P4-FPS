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
    public float speed = 15; //Speed of the player

    [Header ("Camera")]
    public float sensitivity = 50; //Sensitivity of the camera
    public GameObject playerCam; //Camera of the player
    public Vector2 clampRotation; //Min and max clamp
    float camRotation = 0; //Rotation of camera on the X axis.

    void Start () {
        if (!isLocal)
            if (photonView.isMine)
                playerCam.SetActive (true);
            else
                StartCoroutine (LerpPlayer ());
        else
            playerCam.SetActive (true);

    }

    void Update () {
        RotatePlayer ();
        RotateCameraFirstPerson ();
    }

    void FixedUpdate () {
        Move ();
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
            transform.position = Vector3.Lerp (transform.position, networkPosition, networkLerpSmoothing * Time.deltaTime);
            transform.rotation = Quaternion.Lerp (transform.rotation, networkRotation, networkLerpSmoothing * Time.deltaTime);
            yield return null;
        }
    }

    #region MoveAndRotate
    void Move () {

        //Vector to translate to position
        Vector3 movementVector;
        movementVector.x = Input.GetAxis ("Horizontal");
        movementVector.y = 0;
        movementVector.z = Input.GetAxis ("Vertical");

        //Translate the position
        transform.Translate (movementVector * speed * Time.deltaTime);
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