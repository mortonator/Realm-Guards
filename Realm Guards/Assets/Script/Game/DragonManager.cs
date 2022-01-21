using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class DragonManager : NetworkBehaviour
{
    public static DragonManager instance;
    public static short dragonIndex;

    [Header("World")]
    [SerializeField] PortalManager exitPortal;
    [SerializeField] DragonLairSections[] dragonLairs;
    [Header("Boss Data")]
    [SerializeField] TMPro.TMP_Text bossNameText;
    [SerializeField] UnityEngine.UI.Image bossHealthBar;

    [Header("NavMesh")]
    [SerializeField] NavMeshSurface[] surfaces;

    [HideInInspector] public Vector3 dragonPosition;
    [HideInInspector] public float dragonYAngle;
    [HideInInspector] public int currentStrength;
    [HideInInspector] public int strengthKilled;

    [Server] void Start()
    {
        instance = this;

        //Sets up the areas of the lair that are active this combat.
        for (int i = dragonLairs.Length - 1; i >= 0; i--)
        {
            dragonLairs[i].SetLairActive(dragonIndex);
        }

        //Builds nav mesh using active areas of lair.
        BuildNavMesh();

        SpawnDragon();
    }
    [Server] void BuildNavMesh()
    {
        for (int i = surfaces.Length - 1; i >= 0; i--)
        {
            if (surfaces[i].gameObject.activeSelf)
            {
                surfaces[i].BuildNavMesh();
                return;
            }
        }
    }

    void SpawnDragon()
    {
        //Spawns correct dragon based of index.
        Ai_DragonController dragon = Instantiate(EnemyManager.dragonPrefabs[dragonIndex], transform.position, transform.rotation);
        dragon.healthBar = bossHealthBar;
        bossNameText.gameObject.SetActive(true);
        bossHealthBar.gameObject.SetActive(true);
        bossNameText.text = dragon.name.Remove(dragon.name.Length - 7);
        NetworkServer.Spawn(dragon.gameObject);
    }
    [Server] public void DragonKilled()
    {
        exitPortal.gameObject.SetActive(true);
        EnemyManager.Instance.exitSpawned = true;
    }


    public void CollectNavMeshSurfaces() => surfaces = FindObjectsOfType<NavMeshSurface>();
    public void ShowLair_Editor(int dragon)
    {
        //Sets up the areas of the lair that are active this combat.
        for (int i = dragonLairs.Length - 1; i >= 0; i--)
        {
            dragonLairs[i].SetLairActive(dragon);
        }

        //EnemyManager.dragonPrefabs = Resources.LoadAll<Ai_DragonController>("SpawnablePrefabs/Dragons");
        //Instantiate(EnemyManager.dragonPrefabs[dragon].gameObject, transform.position, transform.rotation);
    }
}
[System.Serializable] public class DragonLairSections
{
    [SerializeField] GameObject[] lairObjects;
    [SerializeField] bool lairFor_Black;
    [SerializeField] bool lairFor_Green;
    [SerializeField] bool lairFor_Red;
    [SerializeField] bool lairFor_Yellow;

    public void SetLairActive(int index)
    {
        //if should be active for current dragon index, then bool in array should be true
        bool allow = Allow(index);
        foreach (GameObject item in lairObjects)
            item.SetActive(allow);
    }

    bool Allow(int index)
    {
        if (index == 0) return lairFor_Black;
        if (index == 1) return lairFor_Green;
        if (index == 2) return lairFor_Red;
        return lairFor_Yellow;
    }
}