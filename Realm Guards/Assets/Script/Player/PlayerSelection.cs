using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public static PlayerController[] avatarPrefabs;
    public static IRole[] rolePrefabs;
    public static RangedWeapon[] weaponPrefabs;

    public static int avatarCount;
    public static int weaponCount;
    public static int roleCount;

    void Awake()
    {
        CollectAvatars();
        CollectWeapons();
        CollectRoles();
    }

    static void CollectAvatars()
    {
        avatarPrefabs = Resources.LoadAll<PlayerController>("SpawnablePrefabs/Avatars/");
        avatarCount = avatarPrefabs.Length;
    }
    public static PlayerController GetAvatar(int choice)
    {
        if (avatarPrefabs == null)
            CollectAvatars();

        return avatarPrefabs[choice];
    }    
    public static CharacterData GetAvatarData(int choice)
    {
        if (avatarPrefabs == null)
            CollectAvatars();

        return avatarPrefabs[choice].characterData;
    }

    static void CollectWeapons()
    {
        if (weaponPrefabs == null)
        {
            weaponPrefabs = Resources.LoadAll<RangedWeapon>("SpawnablePrefabs/Weapons/");
            weaponCount = weaponPrefabs.Length;
        }
    }
    public static RangedWeapon GetWeapon(int choice)
    {
        if (weaponPrefabs == null)
            CollectWeapons();

        return weaponPrefabs[choice];
    }
    public static CharacterData GetWeaponData(int choice)
    {
        if (weaponPrefabs == null)
            CollectWeapons();

        return weaponPrefabs[choice].weaponData;
    }

    static void CollectRoles()
    {
        if (rolePrefabs == null)
        {
            rolePrefabs = Resources.LoadAll<IRole>("Roles/");
            roleCount = rolePrefabs.Length;
        }
    }
    public static IRole GetRoles(int choice)
    {
        if (rolePrefabs == null)
            CollectRoles();

        return rolePrefabs[choice];
    }
}
