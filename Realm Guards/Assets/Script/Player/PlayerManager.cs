using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerManager : NetworkBehaviour
{
    public PlayerInput input;
    [HideInInspector] public PlayerController playerCon;
    [HideInInspector] public int avatarSelection = 1;
    [HideInInspector] public int weaponSelection = 0;
    [HideInInspector, SyncVar] public int roleSelection = 0;

    public NetworkConnection conn { get; protected set; }
    bool isLeader;
    bool isSteam;
    [HideInInspector] public bool isPaused = false;

    #region Sync Variables
    [SyncVar(hook = nameof(UpdateDisplay_Name))]
    public string displayName;
    [Command] public void Cmd_SetName(string newName) => displayName = newName;
    void UpdateDisplay_Name(string oldValue, string newValue)
    {
        if (displayName != newValue)
            displayName = newValue;

        Debug.Log("UpdateDisplay_Name");
        UpdateDisplay();
    }

    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    public ulong steamId;
    ulong steamID;
    public Texture2D profileImage = null;
    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    [SyncVar(hook = nameof(UpdateDisplay_Ready))]
    public bool isReady;
    [Command] public void Cmd_SetReady(bool newReady) => isReady = newReady;
    void UpdateDisplay_Ready(bool oldValue, bool newValue)
    {
        Debug.Log("UpdateDisplay_Ready");
        UpdateDisplay();
    }
    [SyncVar(hook = nameof(UpdateDisplay_SceneReady))]
    public bool sceneReady;
    [Command] public void Cmd_SetSceneReady(bool newReady) => sceneReady = newReady;
    void UpdateDisplay_SceneReady(bool oldValue, bool newValue)
    {
        Debug.Log("UpdateDisplay_SceneReady");
    }
    #endregion

    #region Commads
    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        RG_NetworkManager.players.Add(this);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
            OfficeBoard.Instance.AddReady(displayName, netIdentity.isLocalPlayer);
    }
    public override void OnStartAuthority()
    {
        if (OfficeMenu.Instance != null)
            OfficeMenu.Instance.JoinedGame();

        Camera.main.GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera.Priority = 0;
        FindObjectOfType<Cinemachine.CinemachineFreeLook>().Priority = 1;

        GetComponent<PlayerInput>().enabled = true;

        RG_NetworkManager.localPlayer = this;
        SpawnPlayer();
    }
    public override void OnStopClient()
    {
        if (RG_NetworkManager.localPlayer == this)
            RG_NetworkManager.localPlayer = null;

        if (OfficeMenu.Instance != null)
            OfficeMenu.Instance.LeftGame();

        RG_NetworkManager.players.Remove(this);

        if (OfficeBoard.Instance != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)
            OfficeBoard.Instance.RemoveReady();

        base.OnStopClient();
    }

    public void SpawnPlayer()
    {
        Debug.Log("Ask to spawn player. " + netIdentity.hasAuthority);
        CMD_SpawnPlayer(avatarSelection, netIdentity.connectionToClient);
    }
    [Command]
    public void CMD_SpawnPlayer(int a, NetworkConnectionToClient c = null)
    {
        Debug.Log("Spawn player");
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (playerCon != null)
        {
            pos = playerCon.transform.position;
            rot = playerCon.transform.rotation;

            NetworkServer.Destroy(playerCon.gameObject);
        }

        playerCon = Instantiate(PlayerSelection.GetAvatar(a), pos, rot);
        DontDestroyOnLoad(playerCon);
        NetworkServer.Spawn(playerCon.gameObject, c);
        playerCon.SetupPlayer(c, netIdentity);
    }

    public void SetPlayerInfo(bool _isLeader, bool _isSteam = false, ulong _steamID = 0)
    {
        isLeader = _isLeader;
        isSteam = _isSteam;
        steamID = _steamID;
    }
    void UpdateDisplay()
    {
        Debug.Log("UPDATE DISPLAY");
        OfficeBoard.Instance.UpdateReady();
    }

    public PlayerController GetAvatar()
    {
        return PlayerSelection.GetAvatar(avatarSelection);
    }

    [Command] public void StartGame() => RG_NetworkManager.Instance.StartGame();



    [Command] public void Cmd_ChangeMusic()
    {
        if (OfficeMusic.instance != null)
            OfficeMusic.instance.ChangeMusic();
    }
    [Command] public void Cmd_OpenChest(uint id)
    {
        foreach (GadgetBox i in FindObjectsOfType<GadgetBox>())
            if (i.netId == id)
                i.Rpc_OpenChest();
    }
    [Command] public void Cmd_CloseChest(uint id, bool isLeft)
    {
        foreach (GadgetBox i in FindObjectsOfType<GadgetBox>())
            if (i.netId == id)
                i.Rpc_CloseChest(isLeft);
    }
    #endregion

    #region Steam Client
    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        Debug.Log("OnStartClient: Avatar callback set");
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);

        Debug.Log("HandleSteamIdUpdated: steamId =" + newSteamId);
        CSteamID cSteamID = new CSteamID(newSteamId);

        displayName = SteamFriends.GetFriendPersonaName(cSteamID);

        int imageId = SteamFriends.GetLargeFriendAvatar(cSteamID);
        Debug.Log("HandleSteamIdUpdated - imageID = " + imageId.ToString());
        if (imageId == -1)
        {
            return;
        }

        profileImage = GetSteamImageAsTexture(imageId);
        Debug.Log("HandleSteamIdUpdated: profileImage = " + profileImage);
    }

    void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        Debug.Log("OnAvatarImageLoaded: callback steam ID = " + callback.m_steamID.m_SteamID.ToString() + ", this steamID = " + steamId.ToString());
        if (callback.m_steamID.m_SteamID != steamId) return;

        profileImage = GetSteamImageAsTexture(callback.m_iImage);

        UpdateDisplay();
    }

    Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D tex = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                tex.LoadRawTextureData(image);
                tex.Apply();
            }
        }

        return tex;
    }
    #endregion

    #region Input
    public void Do_Jump(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Jump(c);
    }
    public void Do_Move(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Move(c);
    }
    public void Do_Sprint(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Sprint(c);
    }
    public void Do_Fire(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Fire(c);
    }
    public void Do_Reload(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Reload(c);
    }
    public void Do_PauseButton(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.playerUi.DoPause();
    }
    public void Do_Interact(InputAction.CallbackContext c)
    {
        if (playerCon != null)
            playerCon.Do_Interact(c);
    }
    #endregion
}
