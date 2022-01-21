using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Mirror;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(RigBuilder))]
public class PlayerController : NetworkBehaviour, IDamagable
{
    #region Variables
    [SerializeField] Rig aimingRig;
    [SerializeField] Transform cameraLookAt;
    [SerializeField] Transform myAimTarget;
    public PlayerUi playerUi;
    public CharacterData characterData;
    [Header("Character Controller presave")]
    [SerializeField] Vector3 cc_center;
    [SerializeField] float cc_height;
    [SerializeField] float cc_radius;
    [Header("Layers")]
    [SerializeField] LayerMask groundLayerMask = 1;
    const int myPlayerLayer = 6;

    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerManager manager;
    [HideInInspector] public RangedWeapon gun;
    CharacterController characterCont;
    public PlayerData data;

    public IInteractable interactObj { get; protected set; }
    const float interactDistance = 6;

    [Header("Move and Camera")]
    Transform camTrans;
    Transform worldAimTarget;
    Cinemachine.CinemachineFreeLook freeLook;
    bool isPaused;
    bool isSprint;
    Vector2 moveVelocity;
    const float turnSpeed = 15;
    const float aimDuration = 0.15f;

    [Header("Jumping")]
    const float maxVelocity = 15;
    const float gravity = -0.5f;
    bool onGround = true;
    bool doJump = false;
    int jumpCount = 0;
    float upVelocity;
    float moveY;

    [SyncVar(hook = nameof(HandAimingRigWeight))] float aimingRigWeight;
    [Command] void Cmd_SetAimingRigWeight(float newWeight) => aimingRigWeight = newWeight;
    void HandAimingRigWeight(float oldValue, float newValue)
    {
        if (aimingRigWeight != newValue)
            aimingRigWeight = newValue;

        aimingRig.weight = aimingRigWeight;
    }

    [Header("Health")]
    [HideInInspector, SyncVar(hook = nameof(HandleDamage))] public float currentHealth = 100;
    [SyncVar, HideInInspector] public bool isDead = false;

    [Header("Shield")]
    [SyncVar(hook = nameof(HandleDamage))] float currentShield = 100;
    float shieldRecoveryTimer;

    [Header("Revival")]
    float revival_Count = 0;
    float revival_HealthGain_Percentage = 0.25f;
    #endregion

    #region Setup
    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);

        if (TryGetComponent(out Rigidbody r))
            Destroy(r);
    }
    [TargetRpc] public void SetupPlayer(NetworkConnection c, NetworkIdentity manId)
    {
        if (c == null || manId == null)
            return;

        Debug.Log("Setup player");

        manager = manId.GetComponent<PlayerManager>();
        Debug.Log("Manager: " + manager);
        manager.playerCon = this;

        this.enabled = true;
        anim = GetComponent<Animator>();
        playerUi.SetupPlayer();
        aimingRig.weight = 1;
        SetupCamera();

        if (TryGetComponent(out characterCont) == false)
            characterCont = gameObject.AddComponent<CharacterController>();
        characterCont.center = cc_center;
        characterCont.height = cc_height;
        characterCont.radius = cc_radius;

        gameObject.layer = myPlayerLayer;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        data = new PlayerData();
        currentHealth = data.maxHealth;
        moveVelocity = Vector2.zero;
        SpawnWeapon();

        if (manager.isPaused)
            DoPause();
    }
    public void SetupCamera()
    {
        Debug.Log("SetupCamera");
        camTrans = Camera.main.transform;
        worldAimTarget = camTrans.GetChild(0);

        freeLook = FindObjectOfType<Cinemachine.CinemachineFreeLook>();
        if (manager.isPaused)
        {
            freeLook.m_YAxis.m_MaxSpeed = 0;
            freeLook.m_XAxis.m_MaxSpeed = 0;
        }
        freeLook.Follow = transform;
        freeLook.LookAt = cameraLookAt;

        if (gun != null)
            gun.camTrans = camTrans;
    }

    public void SpawnWeapon() => Cmd_SpawnWeapon(manager.weaponSelection);
    [Command] void Cmd_SpawnWeapon(int weaponSelect)
    {
        Debug.Log("SpawnWeapon");
        if (gun != null)
        {
            NetworkServer.Destroy(gun.gameObject);
        }

        gun = Instantiate(PlayerSelection.GetWeapon(weaponSelect), playerUi.gunPivot);
        NetworkServer.Spawn(gun.gameObject, netIdentity.connectionToClient);
        SetupWeapon(gun.netId);
    }
    [ClientRpc] void SetupWeapon(uint gunId)
    {
        Debug.Log("SetupWeapon for " + netIdentity.connectionToServer);
        foreach (NetworkIdentity item in FindObjectsOfType<NetworkIdentity>())
        {
            if (item.netId == gunId)
            {
                gun = item.GetComponent<RangedWeapon>();
                break;
            }
        }

        gun.transform.parent = playerUi.gunPivot;
        gun.playerUi = playerUi;
        if (camTrans != null)
            gun.camTrans = camTrans;

        playerUi.leftHand_IK.position = gun.leftGrip.position;
        playerUi.leftHand_IK.rotation = gun.leftGrip.rotation;
        playerUi.rightHand_IK.position = gun.rightGrip.position;
        playerUi.rightHand_IK.rotation = gun.rightGrip.rotation;

        Debug.Log(gun);
    }
    #endregion

    #region Input
    public void Do_Jump(InputAction.CallbackContext c)
    {
        if (c.performed && jumpCount <= data.jumpMax)
        {
            doJump = true;
        }
    }
    public void Do_Move(InputAction.CallbackContext c)
    {
        moveVelocity = c.ReadValue<Vector2>();

        if (isSprint && moveVelocity.y <= 0)
        {
            isSprint = false;
            anim.SetBool("IsSprint", isSprint);
        }
    }
    public void Do_Sprint(InputAction.CallbackContext c)
    {
        if (c.performed)
        {
            isSprint = !isSprint;
            anim.SetBool("IsSprint", isSprint);
        }
    }
    public void Do_Fire(InputAction.CallbackContext c)
    {
        if (c.performed)
            gun.Fire();
    }
    public void Do_Reload(InputAction.CallbackContext c)
    {
        if (c.performed)
            gun.Reload();
    }
    public void Do_Interact(InputAction.CallbackContext c)
    {
        if (c.performed && interactObj != null)
            interactObj.Interact(this);
    }
    #endregion
    #region Actions
    void FixedUpdate()
    {
        if (!hasAuthority)
            return;


        if (!isPaused)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, camTrans.eulerAngles.y, 0), turnSpeed * Time.fixedDeltaTime);
        }
    }
    void Update()
    {
        characterCont.Move(Vector3.up * upVelocity);

        if (!isPaused && NetworkClient.ready)
        {
            myAimTarget.position = worldAimTarget.position;

            if (isDead == false)
            {
                if (GameManager.Instance.volume.weight != 0)
                {
                    GameManager.Instance.volume.weight = Mathf.Clamp01(GameManager.Instance.volume.weight - Time.deltaTime);
                }

                if (data.maxShield != 0)
                    ShieldRecovery();

                Locomotion();

                CheckRay();
            }
            else
            {
                Revive();
            }
        }
    }
    void Locomotion()
    {
        Movement();
        Jumping();
        Sprinting();
    }

    void Movement()
    {
        anim.SetFloat("Move Z", moveVelocity.y);
        anim.SetFloat("Move X", moveVelocity.x);
    }
    void Sprinting()
    {
        if (isSprint && moveVelocity.y > 0)
        {
            if (aimingRigWeight != 0)
            {
                aimingRig.weight -= Time.deltaTime / aimDuration;
                Cmd_SetAimingRigWeight(aimingRig.weight);
            }
        }
        else
        {
            if (aimingRigWeight != 1)
            {
                aimingRig.weight += Time.deltaTime / aimDuration;
                Cmd_SetAimingRigWeight(aimingRig.weight);
            }
        }
    }

    void Jumping()
    {
        CheckGround();

        if (onGround)
        {
            moveY = 0;
        }
        else
        {
            moveY -= (-gravity * Time.deltaTime) - (upVelocity * Time.deltaTime);
            moveY = Mathf.Clamp(moveY, maxVelocity * -1, maxVelocity * 10);
            if (upVelocity > 0)
            {
                upVelocity += gravity * Time.deltaTime;
            }
            if (upVelocity < 0)
            {
                upVelocity = 0;
            }
        }

        if (doJump)
        {
            upVelocity = data.jumpForce;
            jumpCount++;
            doJump = false;
        }

        //transform.position += movement;
        characterCont.Move(Vector3.up * moveY);
    }
    void CheckGround()
    {
        if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, Vector3.down, out RaycastHit hit, 1.1f, groundLayerMask))
        {
            jumpCount = 0;
            onGround = true;
        }
        else if (onGround)
        {
            jumpCount++;
            onGround = false;
        }
    }


    void CheckRay()
    {
        if (Physics.Raycast(camTrans.position, camTrans.forward, out RaycastHit hit, interactDistance))
            SetInteractable(hit.collider.GetComponent<IInteractable>()); // if null, sends null
        else
            SetInteractable(null);
    }
    public void SetInteractable(IInteractable inter)
    {
        interactObj = inter;

        if (interactObj != null)
            playerUi.SetInteract(interactObj.GetInteractText());
        else
            playerUi.SetInteract(string.Empty);
    }
    #endregion

    public void DoPause(bool pauseMenu = false)
    {
        isPaused = true;

        manager.input.enabled = false;

        freeLook.m_YAxis.m_MaxSpeed = 0;
        freeLook.m_XAxis.m_MaxSpeed = 0;

        playerUi.Pause(pauseMenu);

        anim.SetFloat("Move Z", 0);
        anim.SetFloat("Move X", 0);
        anim.SetBool("IsSprint", false);
    }
    public void UnPause()
    {
        isPaused = false;

        manager.input.enabled = true;

        freeLook.m_YAxis.m_MaxSpeed = 2;
        freeLook.m_XAxis.m_MaxSpeed = 300;

        playerUi.Resume();
    }
    public void Freeze()
    {
        isPaused = true;

        manager.input.enabled = false;

        freeLook.m_YAxis.m_MaxSpeed = 0;
        freeLook.m_XAxis.m_MaxSpeed = 0;

        anim.SetFloat("Move Z", 0);
        anim.SetFloat("Move X", 0);
        anim.SetBool("IsSprint", false);
    }
    public void UnFreeze()
    {
        isPaused = false;

        manager.input.enabled = true;

        freeLook.m_YAxis.m_MaxSpeed = 2;
        freeLook.m_XAxis.m_MaxSpeed = 300;

        RG_NetworkManager.localPlayer.playerCon.playerUi.loadingBarImage.fillAmount = 0;
        RG_NetworkManager.localPlayer.playerCon.playerUi.loadingBarText.text = "Loading";
        RG_NetworkManager.localPlayer.playerCon.playerUi.loadingBarText.transform.parent.gameObject.SetActive(false);
    }

    public void SpawnInNewWorld()
    {
        if (netIdentity.isLocalPlayer == false)
            return;

        anim.SetBool("Die", false);
        currentShield = data.maxShield;

        currentHealth += data.maxHealth * 0.5f;
        currentHealth = Mathf.Clamp(currentHealth, 0, data.maxHealth);
    }
    void ShieldRecovery()
    {
        if (shieldRecoveryTimer != 0)
            shieldRecoveryTimer -= Time.deltaTime;
        else
            currentShield += Time.deltaTime * data.shieldRecoveryMultiplier;
    }

    public void Damage(float damage)
    {
        if (currentShield > 0)
        {
            if (damage > currentShield)
            {
                damage -= currentShield;
                currentShield = 0;
                currentHealth -= damage;
            }
            else
            {
                currentShield -= damage;
            }
        }
        else
        {
            currentHealth -= damage;
        }
    }
    public void HandleDamage(float oldValue, float newValue)
    {
        if (netIdentity.isLocalPlayer == false)
            return;

        if (oldValue > newValue) //has taken damage, not healled.
        {
            shieldRecoveryTimer = data.shieldRecoveryTimerSet;

            if (GameManager.Instance.volume.weight != 1)
            {
                GameManager.Instance.volume.weight += (oldValue - newValue) / data.maxHealth; // the more damage taken, the heavier the greyscale.
                GameManager.Instance.mainMixer.SetFloat("MasterFrequencyGain", 0.8f * GameManager.Instance.volume.weight);

                if (GameManager.Instance.volume.weight > 1)
                {
                    GameManager.Instance.mainMixer.SetFloat("MasterFrequencyGain", 1);
                    GameManager.Instance.volume.weight = 1;
                }
            }
        }
        else
        {
            GameManager.Instance.volume.weight = 0;
        }

        currentShield = Mathf.Clamp(currentShield, 0, data.maxShield);
        currentHealth = Mathf.Clamp(currentHealth, 0, data.maxHealth);



        playerUi.UpdateHealth(currentHealth, data.maxHealth, currentShield, data.maxShield);

        if (currentHealth <= 0)
        {
            anim.SetBool("Die", true);
            playerUi.reviveCircle.SetActive(true);
            isDead = true;

            bool allDead = true;
            foreach (PlayerManager item in RG_NetworkManager.players)
            {
                if (item.playerCon.isDead == false)
                    allDead = false;
            }

            if(allDead)
            {
                // Game Over
                Cmd_GameOver();
            }
        }
    }
    [Command] public void Cmd_GameOver() => RG_NetworkManager.Instance.Ser_GameOver();
    void Revive()
    {
        bool isHelped = false;
        foreach (PlayerManager item in RG_NetworkManager.players)
        {
            if ((item.playerCon.transform.position - transform.position).sqrMagnitude < data.revival_Range)
                isHelped = true;
        }

        if (isHelped)
        {
            revival_Count += data.revival_CountIncrement * Time.deltaTime;
            playerUi.UpdateRevival(revival_Count);

            if (revival_Count >= 1)
            {
                anim.SetBool("Die", false);
                isDead = false;
                currentHealth = data.maxHealth * revival_HealthGain_Percentage;
                playerUi.UpdateRevival(-1);
            }
        }
        else
        {
            revival_Count += data.revival_CountDecrement * Time.deltaTime;
        }
    }
    public void IncreaseRevivalCirclePercentage(float percentageIncrease) // 10% = 0.1f
    {
        // range 0.9 => revivalFX scale 0.04
        // range 10% => revivalFX scale 4.4% of old range

        playerUi.reviveCircle.transform.localScale *= data.revival_Range * (1 + (percentageIncrease * 0.044f));
        data.revival_Range += data.revival_Range * percentageIncrease;
    }
}