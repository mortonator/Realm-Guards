using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : NetworkBehaviour
{
    [SerializeField] AudioSource snd_Fire;
    [SerializeField] AudioSource snd_Empty;
    [SerializeField] AudioSource snd_Reload;
    [Space]
    public CharacterData weaponData;
    [Space]
    [SerializeField] int ammoMax;
    [SerializeField] GameObject muzzleFlash;
    [Space]
    public Transform leftGrip;
    public Transform rightGrip;

    public float damage;
    public float reloadSpeed;
    public float maxDistance = 200;
    int ammo;
    bool isReloading;

    [HideInInspector] public PlayerUi playerUi;
    [HideInInspector] public Transform camTrans;

    public void Fire()
    {
        if (ammo == 0)
        {
            Cmd_PlaySound(1);
        }
        else if (!isReloading)
        {
            Cmd_PlaySound(0);
            ammo--;
            playerUi.UpdateAmmo(ammo, ammoMax);
            Shoot();
        }
    }
    void Shoot()
    {
        CMD_MuzzleFlash();

        if (Physics.Raycast(camTrans.position, camTrans.forward, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.gameObject.TryGetComponent(out IDamagable dam))
            {
                dam.Damage(damage);
            }
        }
    }

    [Command] void CMD_MuzzleFlash() => RPC_MuzzleFlash();
    [ClientRpc] void RPC_MuzzleFlash() => StartCoroutine(DoMuzzleFlash());
    IEnumerator DoMuzzleFlash()
    {
        muzzleFlash.transform.localEulerAngles = new Vector3(Random.Range(0, 360), 90, -90);
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        muzzleFlash.SetActive(false);
    }

    public void Reload()
    {
        if (isReloading)
            return;

        isReloading = true;
        ammo = 0;
        playerUi.UpdateAmmo(ammo, ammoMax);

        Cmd_PlaySound(2);
        StartCoroutine(DoReload());
    }
    IEnumerator DoReload()
    {
        yield return new WaitForSeconds(reloadSpeed);
        ammo = ammoMax;
        isReloading = false;
        playerUi.UpdateAmmo(ammo, ammoMax);
    }

    [Command] void Cmd_PlaySound(int index) => Rpc_PlaySound(index);
    [ClientRpc] void Rpc_PlaySound(int index)
    {
        if (index == 0)
            snd_Fire.Play();
        else if (index == 1)
            snd_Empty.Play();
        else if (index == 2)
            snd_Reload.Play();
    }

    public void IncreaseAmmoMax(int inc)
    {
        ammoMax += inc;
        ammo += inc;

        playerUi.UpdateAmmo(ammo, ammoMax);
    }
}
