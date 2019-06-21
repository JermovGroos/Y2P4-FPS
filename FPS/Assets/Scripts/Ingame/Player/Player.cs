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
    public List<SkinnedMeshRenderer> shadowsOnlyIfLocal; //List of skinned mesh renderers to put on shadows-only for player viewport

    [Header("Health")]
    public float mainHealth = 100; //Main health of the player
    public float health; //Current health of the player
    public Armor playerArmor; //Armor of the player
    public List<Damage> damages = new List<Damage>();

    [Header("Colliders")]
    public Vector3 standingColliderCenter; //Center of the box collider when standing
    public Vector3 standingColliderSize; //Size of the box collider when standing
    public Vector3 crouchingColliderCenter; //Center of the box collider when crouching
    public Vector3 crouchingColliderSize; //Size of the box collider when crouching

    [Header("Movement")]
    public float walkSpeed = 5; //Speed of the player when walking
    public float runSpeed = 10; //Speed of the player when running
    public float crouchSpeed = 2.75f; //Speed of the player when crouching
    public float speedDampTime = 0.25f; //Time to damp the speed with
    public bool movementAllowed = true; //Check if movement is allowed
    bool isGrounded; //Check if grounded
    Vector3 movementVector; //Movement vector of player

    [Space(10)]
    public float jumpForce = 250; //Force of the jump
    public Vector3 jumpBoxCheckSize; //Size of the box that detects jumpable layers
    public LayerMask playerLayer; //Layer of the player
    Rigidbody playerRigidbody; //Rigidbody of the player

    [Header("Damage")]
    public float meleeRange = 1;
    public float meleeDamage = 10;
    public float meleeCooldown = 3;
    bool canMelee = true;

    [Header("Scene")]
    public string sceneCameraTag = "SceneCamera"; //Tag of the scene camera
    GameObject sceneCam; //Camera of the scene

    [Header("Camera")]
    public GameObject playerCam; //Camera of the player
    public int sensitivity = 100; //Sensitivity of the camera
    public Vector2 clampRotation; //Min and max clamp
    public float standingCamHeight = 1.75f; //Height of camera when standing
    public float crouchingCamHeight = 1.15f; //Position of camera joint when crouching
    public float camDampTime = 0.05f; //Damp time of camera
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
    public Image healthBar; //Healthbar of the player
    public Color teammateSpriteColor = Color.blue; //Color to make your teammates sprites
    public Color enemySpriteColor = Color.red; //Color to make your enemies sprites
    Scoreboard scoreboard; //Scoreboard manager

    [Header("Managers")]
    public string gameInfoManagerTag = "Manager"; //Tag of the game info manager gameobject
    GameInfoManager gameInfoManager; //Game info manager
    ManagerBasicStuff basicManager;

    void Start()
    {

        StartCoroutine(FindSceneCam());

        //Set player rigidbody
        playerRigidbody = GetComponent<Rigidbody>();

        //Set player health
        health = mainHealth;

        //Set game info manager
        if (GameObject.FindWithTag(gameInfoManagerTag))
        {
            GameObject manager = GameObject.FindWithTag(gameInfoManagerTag);
            gameInfoManager = manager.GetComponent<GameInfoManager>();
            basicManager = manager.GetComponent<ManagerBasicStuff>();
            scoreboard = manager.GetComponent<Scoreboard>();

            healthBar = basicManager.healthBar;
        }

        if (!isLocal)
            if (photonView.isMine)
            {
                print("Photon View is mine");
                IsMineOrLocal();

                //Set bar to 100%
                healthBar.fillAmount = 1;
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

    IEnumerator FindSceneCam()
    {
        bool isFound = false;

        while (!isFound)
        {
            //Set scene camera
            sceneCam = GameObject.FindGameObjectWithTag(sceneCameraTag);

            if (sceneCam != null)
            {
                isFound = true;
            }

            yield return null;
        }

        yield break;
    }

    public void IsMineOrLocal()
    {
        TurnOnCamera(true);
        minimapCam.SetActive(true);
        playerRigidbody.useGravity = true;

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in shadowsOnlyIfLocal)
        {
            skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    void TurnOnCamera(bool onOrOff)
    {

        //Change camera
        if (onOrOff)
        {
            playerCam.SetActive(true);
        }
        else
        {
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

            if (scoreboard != null)
            {
                if (Input.GetButton("Scoreboard"))
                    scoreboard.scoreboardUI.SetActive(true);
                else
                    scoreboard.scoreboardUI.SetActive(false);

            }

            if (Input.GetButtonDown("Melee") && canMelee)
            {
                Melee();
            }

            //TEMPORARY
            if (Input.GetButtonDown("Temp"))
            {
                TurnOnCamera(!camToggle);
                camToggle = !camToggle;
            }

            if (transform.position.y <= -40)
                GameObject.FindWithTag(gameInfoManagerTag).GetComponent<GameInfoManager>().Respawn();
        }

    }

    void FixedUpdate()
    {
        if (photonView.isMine)
            if (movementAllowed)
            {

                //Move();
                NewMove();
            }
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
            animator.SetTrigger("Grounded");

            //Check if player presses the Jump button
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetTrigger("Jump");

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

    Vector2 currentSpeed;
    float xSpeedSmoothVelocity;
    float ySpeedSmoothVelocity;
    float camSpeedSmoothVelocity;
    float currentCamHeight;
    bool isCrouched;

    void NewMove()
    {

        //Retrieve and normalize input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;

        //Make nesseccary Vector2 
        Vector2 animatorSpeed = Vector2.zero;
        Vector2 targetSpeed = Vector2.zero;
        float targetHeight = 0f;

        //Get isCrouched
        isCrouched = Input.GetButton("Crouch");

        if (isCrouched)
        {
            //Set target speed and damp to it
            targetSpeed = new Vector2(crouchSpeed * input.x, crouchSpeed * input.y);
            currentSpeed.x = Mathf.SmoothDamp(currentSpeed.x, targetSpeed.x, ref xSpeedSmoothVelocity, speedDampTime);
            currentSpeed.y = Mathf.SmoothDamp(currentSpeed.y, targetSpeed.y, ref ySpeedSmoothVelocity, speedDampTime);

            //Set animator speed
            animatorSpeed = inputDir * (0.5f);

            //Set crouch bool
            animator.SetBool("Crouched", true);

            //Set cam pos
            Vector3 localCamPos = playerCam.transform.localPosition;
            targetHeight = crouchingCamHeight;
            localCamPos.y = Mathf.SmoothDamp(localCamPos.y, targetHeight, ref camSpeedSmoothVelocity, camDampTime);
            playerCam.transform.localPosition = localCamPos;



        }
        else
        {
            //Check for sprint
            bool running = Input.GetButton("Sprint");

            //Set target speed and damp to it
            targetSpeed = new Vector2(((running) ? runSpeed : walkSpeed) * input.x, ((running) ? runSpeed : walkSpeed) * input.y);
            currentSpeed.x = Mathf.SmoothDamp(currentSpeed.x, targetSpeed.x, ref xSpeedSmoothVelocity, speedDampTime);
            currentSpeed.y = Mathf.SmoothDamp(currentSpeed.y, targetSpeed.y, ref ySpeedSmoothVelocity, speedDampTime);

            //Set animator speed
            animatorSpeed = inputDir * ((running) ? 1 : 0.5f);

            //Set crouch bool
            animator.SetBool("Crouched", false);

            //Set cam pos
            Vector3 localCamPos = playerCam.transform.localPosition;
            targetHeight = standingCamHeight;
            localCamPos.y = Mathf.SmoothDamp(localCamPos.y, targetHeight, ref camSpeedSmoothVelocity, camDampTime);
            playerCam.transform.localPosition = localCamPos;

        }


        //Apply animator variables to animator
        animator.SetFloat("Forward", animatorSpeed.y, speedDampTime, Time.deltaTime);
        animator.SetFloat("Side", animatorSpeed.x, speedDampTime, Time.deltaTime);

        //Get final movement vector
        Vector3 final = new Vector3(targetSpeed.x, 0f, targetSpeed.y) * Time.deltaTime;

        //Translate final position
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

            //Set health bar fill
            float barAmount = health / mainHealth;
            print("Healthbar fill percentage: " + barAmount.ToString());
            healthBar.fillAmount = barAmount;

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

                GameObject.FindWithTag(gameInfoManagerTag).GetComponent<PhotonView>().RPC("PlayerKilled", PhotonTargets.MasterClient, PhotonNetwork.playerName, damager, damageAmounts, damagers);

                if (gameInfoManager.allowRespawn || gameInfoManager.currentRoundType == GameInfoManager.RoundType.Waiting || gameInfoManager.currentRoundType == GameInfoManager.RoundType.Warmup)
                    gameInfoManager.Respawn();
                else
                    PhotonNetwork.Destroy(gameObject);

            }

        }

    }
    #endregion


    void Melee()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, meleeRange, playerLayer))
        {
            hit.transform.GetComponent<Player>().DamagePlayer(PhotonNetwork.playerName, meleeDamage);
            StartCoroutine(WaitForMelee(meleeCooldown));
            print("Melee!");
        }
    }

    IEnumerator WaitForMelee(float seconds)
    {
        canMelee = false;
        yield return new WaitForSeconds(seconds);
        canMelee = true;
    }

    void SetMinimapColors(List<Player> teammates, List<Player> enemies)
    {

        //Set teammate color
        foreach (Player teammate in teammates)
            teammate.minimapSprite.color = teammateSpriteColor;

        //Set enemy color
        foreach (Player enemy in enemies)
            enemy.minimapSprite = null;

    }
}