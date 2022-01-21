using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUi : MonoBehaviour
{
    [Header("Screen")]
    [SerializeField] GameObject gameScreen;
    [SerializeField] GameObject pauseScreen;
    [Header("Setup")]
    [SerializeField] Rig bodyRig;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject cameraControls;
    [Header("Gun Rig")]
    public Transform gunPivot;
    public Transform leftHand_IK;
    public Transform rightHand_IK;
    [Header("Game UI")]
    [SerializeField] TMP_Text interactText;
    [SerializeField] TMP_Text ammoText;
    [Header("Health & Shield")]
    [SerializeField] TMP_Text healthText;
    [SerializeField] Image healthImage;
    [SerializeField] TMP_Text shieldText;
    [SerializeField] Image shieldImage;
    [Header("Revival")]
    public GameObject reviveCircle;
    [SerializeField] Image revivalImage;
    [Header("Loading")]
    public Image loadingBarImage;
    public TMP_Text loadingBarText;

    PlayerController playerCont;
    bool isPause = false;

    public void SetupPlayer()
    {
        this.enabled = true;
        playerCont = GetComponentInParent<PlayerController>();

        bodyRig.weight = 1;
        canvas.SetActive(true);
        cameraControls.SetActive(true);
    }

    public void SetInteract(string interact)
    {
        if (string.IsNullOrWhiteSpace(interact) == false)
        {
            if (interact.StartsWith("[]"))
                interactText.text = interact.TrimStart("[]".ToCharArray());
            else
                interactText.text = "E " + interact;
        }
        else
            interactText.text = string.Empty;
    }

    #region Menu
    public void Do_PauseButton()
    {
        if (isPause)
            playerCont.UnPause();
        else
            playerCont.DoPause(true);
    }

    public void DoPause() => playerCont.DoPause(true);
    public void Pause(bool pauseMenu)
    {
        isPause = true;
        if (pauseMenu)
            pauseScreen.SetActive(true);
        gameScreen.SetActive(false);

        if (GameManager.Instance.worldGameCanvas != null)
            GameManager.Instance.worldGameCanvas.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = SettingsController.lockState;
    }
    public void DoResume() => playerCont.UnPause();
    public void Resume()
    {
        isPause = false;
        gameScreen.SetActive(true);
        pauseScreen.SetActive(false);
        if (GameManager.Instance.worldGameCanvas != null)
            GameManager.Instance.worldGameCanvas.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void QuitToMenu()
    {
        if (NetworkClient.isHostClient)
            RG_NetworkManager.Instance.StopHost();
        else
            RG_NetworkManager.Instance.StopClient();
    }
    #endregion

    #region Game
    public void UpdateHealth(float health, float maxHealth, float shield, float maxShield)
    {
        healthText.text = health + "/" + maxHealth;
        healthImage.fillAmount = health / maxHealth;
        healthImage.color = Color.Lerp(Color.red, Color.green, healthImage.fillAmount);

        if (maxShield != 0)
        {
            shieldText.text = shield + "/" + maxShield;
            shieldImage.fillAmount = shield / maxShield;
            shieldImage.color = Color.Lerp(Color.blue, Color.cyan, shieldImage.fillAmount);
        }
    }
    public void UpdateAmmo(int ammo, int maxAmmo)
    {
        ammoText.text = ammo + "/" + maxAmmo;
    }

    public void UpdateRevival(float count)
    {
        if (count == -1)
        {
            revivalImage.transform.parent.gameObject.SetActive(false);
            revivalImage.fillAmount = 0;
        }
        else
        {
            revivalImage.transform.parent.gameObject.SetActive(true);
            revivalImage.fillAmount = count;
        }
    }
    #endregion
}
