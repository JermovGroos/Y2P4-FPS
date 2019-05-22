using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Photon.MonoBehaviour
{

    [Header("Networking")]
    public bool isLocal = false; //Is playing locally
    public float networkLerpSmoothing = 10; //Smoothing of network player
    Vector3 networkPosition; //Position of network player
    Quaternion networkRotation; //Rotation of network player

    [Header("Health")]
    public float mainHealth = 100; //Main health of the player
    public float health; //Current health of the player
    public Armor playerArmor; //Armor of the player
    public List<Damage> damages = new List<Damage>();

    [Header("Movement")]
    public float mainSpeed = 15; //Speed of the player
    public float sprintMultiplier = 1.3f; //Multiplication of speed when sprinting
    public float crouchMultiplier = 0.75f; //Multiplication of speed when crouching
    public float inAirMultiplier = 0.2f; //Multiplication of speed when in-air
    public bool movementAllowed = true; //Check if movement is allowed
    bool isGrounded; //Check if grounded
    Vector3 movementVector; //Movement vector of player

    [Space(10)]
    public float jumpForce = 250; //Force of the jump
    public Vector3 jumpBoxCheckSize; //Size of the box that detects jumpable layers
    public LayerMask playerLayer; //Layer of the player
    Rigidbody playerRigidbody; //Rigidbody of the player

    [Header("Scene")]
    public string sceneCameraTag = "SceneCamera"; //Tag of the scene camera
    GameObject sceneCam; //Camera of the scene

    [Header("Camera")]
    public GameObject playerCam; //Camera of the player
    public int sensitivity = 100; //Sensitivity of the camera
    public Vector2 clampRotation; //Min and max clamp
    public GameObject minimapCam; //Camera of the minimap
    float camRotation = 0; //Rotation of camera on the X axis.
    bool camToggle; //Toggle camera bool

    [Header("Animations")]
    public Animator animator; //Animator of the player  
    public string idleTrigger = "Idle"; //Name of idle animation trigger
    public string walkingTrigger = "Walk"; //Name of idle animation trigger
    public string sprintingTrigger = "Sprint"; //Name of idle animation trigger

    [Header("UI")]
    public SpriteRenderer minimapSprite; //Sprite of the player on the minimap
    public Color teammateSpriteColor = Color.blue; //Color to make your teammates sprites
    public Color enemySpriteColor = Color.red; //Color to make your enemies sprites
    Scoreboard scoreboard; //Scoreboard manager

    [Header("Managers")]
    public string gameInfoManagerTag = "Manager"; //Tag of the game info manager gameobject
    GameInfoManager gameInfoManager; //Game info manager

    void Start()
    {

        //Set scene camera
        sceneCam = GameObject.FindGameObjectWithTag(sceneCameraTag);

        //Set player rigidbody
        playerRigidbody = GetComponent<Rigidbody>();

        //Set player health
        health = mainHealth;

        //Set game info manager
        if (GameObject.FindWithTag(gameInfoManagerTag))
        {
            GameObject manager = GameObject.FindWithTag(gameInfoManagerTag);
            gameInfoManager = manager.GetComponent<GameInfoManager>();
            scoreboard = manager.GetComponent<Scoreboard>();
        }

        if (!isLocal)
            if (photonView.isMine)
            {
                print("Photon View is mine");
                IsMineOrLocal();
            }
            else
            {
                print("Photon View isn't mine");
                StartCoroutine(LerpPlayer());
            }
        else
        {
            print("Is Local");
            IsMineOrLocal();
        }

    }

    public void IsMineOrLocal()
    {
        ChangeCam(true);
        minimapCam.SetActive(true);
        playerRigidbody.useGravity = true;
    }

    void ChangeCam(bool changeToPlayerCam)
    {

        //Change camera
        if (changeToPlayerCam)
        {

            //From scene to player
            sceneCam.SetActive(false);
            playerCam.SetActive(true);
        }
        else
        {

            //From player to scene
            sceneCam.SetActive(true);
            playerCam.SetActive(false);
        }
    }

    void Update()
    {
        if (photonView.isMine)
        {
            RotatePlayer();
            RotateCameraFirstPerson();

            if (movementAllowed)
                Jumper();

            if(scoreboard != null)
            {
                if(Input.GetButton("Scoreboard"))
                    scoreboard.scoreboardUI.SetActive(true);
                 else
                    scoreboard.scoreboardUI.SetActive(false);
                
            }

            //TEMPORARY
            if (Input.GetButtonDown("Temp"))
            {
                ChangeCam(!camToggle);
                camToggle = !camToggle;
            }
        }

    }

    void FixedUpdate()
    {
        if (photonView.isMine)
            if (movementAllowed)
                Move();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position); //Send player position
            stream.SendNext(transform.rotation); //Send player rotation
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext(); //Receive player position
            networkRotation = (Quaternion)stream.ReceiveNext(); //Receive player rotation
        }

    }

    #region Movement

    public IEnumerator LerpPlayer()
    {
        while (true)
        {
            if (!isLocal)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, networkLerpSmoothing * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, networkLerpSmoothing * Time.deltaTime);
            }
            yield return null;
        }
    }

    public void Jumper()
    {

        //Set layermask
        LayerMask notPlayer = ~playerLayer;

        Vector3 halfExtends = jumpBoxCheckSize / 2f;

        //Check if the player is grounded
        if (Physics.CheckBox(transform.position, halfExtends, transform.rotation, notPlayer))
        {

            //Set isGrounded bool true
            isGrounded = true;

            //Check if player presses the Jump button
            if (Input.GetButtonDown("Jump"))
            {

                //Add force to the player rigidbody
                playerRigidbody.AddRelativeForce(Vector3.up * jumpForce + movementVector * jumpForce, ForceMode.Impulse);
            }
        }
        else
        {

            //Set isGrounded bool false
            isGrounded = false;
        }
    }

    void Move()
    {

        //Vector to translate to position
        movementVector.x = Input.GetAxis("Horizontal");
        movementVector.y = 0;
        movementVector.z = Input.GetAxis("Vertical");

        //Check multipliers
        if (!isGrounded)
            movementVector *= inAirMultiplier; //Add in air multiplier
        else if (Input.GetButton("Sprint"))
            movementVector *= sprintMultiplier; //Add sprinting multiplier
        else if (Input.GetButton("Crouch"))
            movementVector *= crouchMultiplier; //Add crouching multiplier

        movementVector *= playerArmor.speedMultiplier; //Add armor multiplier

        //Set the final Vector
        Vector3 final = movementVector * mainSpeed * Time.deltaTime;

        // //Set animation triggers
        // if (Input.GetAxis("Vertical") != 0)
        //     if (Input.GetButton("Sprint"))
        //        animator.SetTrigger(sprintingTrigger);
        //    else
        //        animator.SetTrigger(walkingTrigger);
        // else
        //    animator.SetTrigger(idleTrigger);

        //Translate the position
        transform.Translate(final);

    }

    void RotatePlayer()
    {

        //Rotate the player
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime);
    }

    void RotateCameraFirstPerson()
    {

        //Set new X axis rotation
        camRotation += -1 * Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        camRotation = Clamp(camRotation, clampRotation.x, clampRotation.y);

        //Rotate cam on X axis
        Vector3 cameraRotation = playerCam.transform.localRotation.eulerAngles;
        cameraRotation.x = camRotation;
        playerCam.transform.localRotation = Quaternion.Euler(cameraRotation);
    }

    float Clamp(float input, float min, float max)
    {
        if (input < min)
            return min;
        else if (input > max)
            return max;
        else
            return input;
    }
    #endregion

    #region Health

    [PunRPC]
    public void DamagePlayer(string damager, float damageAmount)
    {
        if (photonView.isMine)
        {

            //Subtract damage from health
            health -= damageAmount;

            //Check if player already got damaged by damager     
            bool containsDamage = false;
            foreach (Damage damage in damages)
            {
                if (damage.damager == damager)
                {

                    //Add damageamount to damager's damage
                    containsDamage = true;
                    damage.amount += damageAmount;
                    break;
                }
            }

            //Add new damage
            if (!containsDamage)
                damages.Add(new Damage(damager, damageAmount));

            //Print to console
            print(transform.name + " took " + damageAmount + " damage by " + damager + ".");

            //Check if player has died
            if (health <= 0)
            {

                //Make lists of the damages
                float[] damageAmounts = new float[damages.Count];
                string[] damagers = new string[damages.Count];

                int index = 0;
                foreach (Damage damage in damages)
                {
                    damageAmounts[index] = damage.amount;
                    damagers[index] = damage.damager;
                    index += 1;
                }

                if (gameInfoManager.allowRespawn || gameInfoManager.currentRoundType == GameInfoManager.RoundType.Waiting || gameInfoManager.currentRoundType == GameInfoManager.RoundType.Warmup)
                    gameInfoManager.Respawn();
                else
                    PhotonNetwork.Destroy(gameObject);

            }

        }

    }
    #endregion

    void SetMinimapColors(List<Player> teammates, List<Player> enemies)
    {

        //Set teammate color
        foreach (Player teammate in teammates)
            teammate.minimapSprite.color = teammateSpriteColor;

        //Set enemy color
        foreach (Player enemy in enemies)
            enemy.minimapSprite.color = enemySpriteColor;

    }
}