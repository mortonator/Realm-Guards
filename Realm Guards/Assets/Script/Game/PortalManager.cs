using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : NetworkBehaviour, IInteractable
{
    public string interactText = " to go to next world";
    public string GetInteractText() => interactText;

    public Cinemachine.ICinemachineCamera mainCamera { get; protected set; }
    [SerializeField] Cinemachine.CinemachineVirtualCamera portalCamera1;
    [SerializeField] Cinemachine.CinemachineVirtualCamera portalCamera2;
    [Space]
    [SerializeField] Transform portalRotate;

    UnityEngine.UI.Image portalLoadingBar;
    TMPro.TMP_Text portalLoadingText;

    const float spinSpeed = 2;

    bool show = false;
    string newSceneName;
    float progress;

    public void Interact(PlayerController p)
    {
        if (RG_NetworkManager.Instance.IsReadyToStart())
        {
            p.manager.StartGame();
        }
    }

    void FixedUpdate()
    {
        if (show && NetworkManager.loadingSceneAsync != null)
        {
            portalRotate.localEulerAngles += Vector3.forward * spinSpeed;

            progress = Mathf.Clamp01(NetworkManager.loadingSceneAsync.progress / 0.9f);
            portalLoadingBar.fillAmount = progress;
            portalLoadingText.text = $"Loading {newSceneName}... {(int)(progress * 100)}%";

            if (NetworkManager.loadingSceneAsync.isDone)
                LoadingComplete();
        }
    }
    
    [Client] public void BeginSceneTransition(string _newSceneName)
    {
        Debug.Log("Begin Scene Transition1: open door");
        RG_NetworkManager.localPlayer.playerCon.SetInteractable(null);
        DontDestroyOnLoad(RG_NetworkManager.localPlayer.playerCon.gameObject);
        GetComponent<Animator>().SetBool("Open", true);

        Debug.Log("Begin Scene Transition2: setup camera");
        if (mainCamera == null)
            mainCamera = Camera.main.GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera;
        mainCamera.Priority = 0;

        Debug.Log("Begin Scene Transition3: assign which camera");
        if ((portalCamera1.transform.position - RG_NetworkManager.localPlayer.transform.position).sqrMagnitude < (portalCamera2.transform.position - RG_NetworkManager.localPlayer.transform.position).sqrMagnitude)
            portalCamera1.Priority = 1;
        else
            portalCamera2.Priority = 1;

        Debug.Log("Begin Scene Transition4: set loading ui");
        portalLoadingBar = RG_NetworkManager.localPlayer.playerCon.playerUi.loadingBarImage;
        portalLoadingBar.fillAmount = 0;

        string[] split = _newSceneName.Split('/');
        newSceneName = split[split.Length - 1].Split('.')[0];
        portalLoadingText = RG_NetworkManager.localPlayer.playerCon.playerUi.loadingBarText;
        portalLoadingText.transform.parent.gameObject.SetActive(true);
        portalLoadingText.text = $"Loading {newSceneName}... 0%";

        RG_NetworkManager.localPlayer.playerCon.Freeze();
        show = true;

        Debug.Log("Begin Scene Transition5: reset player transform");
        RG_NetworkManager.localPlayer.playerCon.transform.position = Vector3.zero;
        RG_NetworkManager.localPlayer.playerCon.transform.eulerAngles = Vector3.zero;

        Debug.Log("Begin Scene Transition6: funciton");
    }
    void LoadingComplete()
    {
        Debug.Log("Loading Scene Complete");
        show = false;
        portalLoadingText.text = $"Loading {newSceneName} Complete. Waiting for other players";
    }
}
