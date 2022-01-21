using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Area")]
    [SerializeField] Vector3 center;
    [SerializeField] Vector3 size = Vector3.one;
    [Header("World")]
    [SerializeField] bool dragonFight = false;
    [SerializeField] PortalManager exitPortal;
    [Header("Boss Data")]
    [SerializeField] TMPro.TMP_Text bossNameText;
    [SerializeField] UnityEngine.UI.Image bossHealthBar;

    public const int updateIncrement = 50;

    public static Ai_BasicController[] aiPrefabs;
    public static Ai_BasicController[] bossPrefabs;
    public static Ai_BasicController[] minionPrefabs;
    public static Ai_DragonController[] dragonPrefabs;
    public static GameObject prefabDeathExplosion;

    [HideInInspector] public List<Ai_BasicController> aiControllers;
    [HideInInspector] public int currentStrength;
    [HideInInspector] public int strengthKilled;
    int strengthCapacity;
    bool canSpawnAi;

    Ai_BasicController currentBoss;
    bool bossSpawned;
    [HideInInspector] public bool exitSpawned;
    [HideInInspector] public bool bossKilled = false;
    [HideInInspector] public Vector3 bossLocation;
    [HideInInspector] public float bossYAngle;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawCube(center, size * 2);
    }

    [Server] void Start()
    {
        Instance = this;

        bossNameText.gameObject.SetActive(false);
        bossHealthBar.gameObject.SetActive(false);

        GetCapacity();
        GetPrefabs();

        if (!dragonFight)
            SpawnAllAi();
        else
            SpawnAllAi_Minions();
    }
    void GetCapacity()
    {
        currentStrength = 0;

        if (!dragonFight)
        {
            if (MapHandler.currentMapNumber == 0 || MapHandler.currentMapNumber == 1)
            {
                strengthCapacity = (MapHandler.currentMapNumber * 30) + 50;
            }
            else
            {
                strengthCapacity = MapHandler.currentMapNumber / 4;
                strengthCapacity *= strengthCapacity * 150;
                strengthCapacity += 50;
            }
        }
        else
        {
            strengthCapacity = (DragonManager.dragonIndex * 20) + 30;
        }
    }
    public static void GetPrefabs()
    {
        if (aiPrefabs == null)
            aiPrefabs = Resources.LoadAll<Ai_BasicController>("SpawnablePrefabs/Ai/");

        if (bossPrefabs == null)
            bossPrefabs = Resources.LoadAll<Ai_BasicController>("SpawnablePrefabs/Boss/");

        if (prefabDeathExplosion == null)
            prefabDeathExplosion = Resources.Load<GameObject>("SpawnablePrefabs/DeathExplosion");

        if (dragonPrefabs == null)
            dragonPrefabs = Resources.LoadAll<Ai_DragonController>("SpawnablePrefabs/Dragons");

        if (minionPrefabs== null)
            minionPrefabs = Resources.LoadAll<Ai_BasicController>("SpawnablePrefabs/Dragons/Minions");
    }

    void SpawnAllAi()
    {
        aiControllers = new List<Ai_BasicController>();
        List<int> aiList = new List<int>();
        int rng;
        int attemptCount = 0;

        Debug.Log("SpawnAi: prefab count = " + aiPrefabs.Length);
        while (currentStrength != strengthCapacity && aiList.Count < 21)
        {
            rng = Random.Range(0, aiPrefabs.Length);

            if ((currentStrength + aiPrefabs[rng].strength) <= strengthCapacity)
            {
                aiList.Add(rng);
                currentStrength += aiPrefabs[rng].strength;
            }
            else
            {
                attemptCount++;
                if (attemptCount > 100)
                {
                    Debug.LogWarning($"SpawnAi failed to spawn ai. Remaining strength {currentStrength} and {aiControllers.Count} ai has been successfully spawned. Last attempt {rng} with strength {aiPrefabs[rng].strength}");
                    break;
                }
            }
        }

        for (int i = aiList.Count - 1; i >= 0; i--)
        {
            aiControllers.Add(Instantiate(aiPrefabs[i], GetRandomPosition(), Quaternion.identity));
            NetworkServer.Spawn(aiControllers[aiControllers.Count - 1].gameObject);
        }

        StartCoroutine(CanSpawnAiTimer());
    }    
    void SpawnAllAi_Minions()
    {
        aiControllers = new List<Ai_BasicController>();
        List<int> aiList = new List<int>();
        int rng;
        int attemptCount = 0;

        Debug.Log("SpawnAi_Minions: prefab count = " + minionPrefabs.Length);
        while (currentStrength != strengthCapacity && aiList.Count < 21)
        {
            rng = Random.Range(0, minionPrefabs.Length);

            if ((currentStrength + minionPrefabs[rng].strength) <= strengthCapacity)
            {
                aiList.Add(rng);
                currentStrength += minionPrefabs[rng].strength;
            }
            else
            {
                attemptCount++;
                if (attemptCount > 100)
                {
                    Debug.LogWarning($"SpawnAi failed to spawn ai. Remaining strength {currentStrength} and {aiControllers.Count} ai has been successfully spawned. Last attempt {rng} with strength {aiPrefabs[rng].strength}");
                    break;
                }
            }
        }

        for (int i = aiList.Count - 1; i >= 0; i--)
        {
            aiControllers.Add(Instantiate(minionPrefabs[i], GetRandomPosition(), Quaternion.identity));
            NetworkServer.Spawn(aiControllers[aiControllers.Count - 1].gameObject);
        }

        StartCoroutine(CanSpawnAiTimer());
    }
    void SpawnAnAi()
    {
        if (currentStrength >= strengthCapacity)
            return;

        int rng;
        int attempt = 0;

        if (!dragonFight)
        {
            do
            {
                rng = Random.Range(0, aiPrefabs.Length);
                attempt++;
            } while (((currentStrength + aiPrefabs[rng].strength) > strengthCapacity) && (attempt < 50));

            aiControllers.Add(Instantiate(aiPrefabs[rng], GetRandomPosition(), Quaternion.identity));
            NetworkServer.Spawn(aiControllers[aiControllers.Count - 1].gameObject);
            currentStrength += aiPrefabs[rng].strength;
        }
        else
        {
            do
            {
                rng = Random.Range(0, minionPrefabs.Length);
                attempt++;
            } while (((currentStrength + minionPrefabs[rng].strength) > strengthCapacity) && (attempt < 50));

            aiControllers.Add(Instantiate(minionPrefabs[rng], GetRandomPosition(), Quaternion.identity));
            NetworkServer.Spawn(aiControllers[aiControllers.Count - 1].gameObject);
            currentStrength += minionPrefabs[rng].strength;
        }

        StartCoroutine(CanSpawnAiTimer());
    }
    IEnumerator CanSpawnAiTimer()
    {
        canSpawnAi = false;
        yield return new WaitForSeconds(5);
        canSpawnAi = true;
    }

    void SpawnBoss()
    {
        int bossIndex = Random.Range(0, bossPrefabs.Length);

        bossNameText.gameObject.SetActive(true);
        bossHealthBar.gameObject.SetActive(true);
        bossNameText.text = bossPrefabs[bossIndex].name;

        currentBoss = Instantiate(
            bossPrefabs[bossIndex],
            GetRandomPosition(),
            Quaternion.Euler(0, Random.Range(0, 360), 0)
        );
        NetworkServer.Spawn(currentBoss.gameObject);
        currentBoss.healthBar = bossHealthBar;
        bossSpawned = true;
    }
    void SpawnExit()
    {
        exitSpawned = true;
        NetworkServer.Spawn(Instantiate(exitPortal, bossLocation, Quaternion.Euler(0, bossYAngle + 90, 0)).gameObject);
    }
    public Vector3 GetRandomPosition()
    {
        Vector3 newPos = Vector3.zero;
        NavMeshHit navHit;

        while (newPos == Vector3.zero)
        {
            newPos = new Vector3(center.x + Random.Range(-size.x, size.x), center.y + Random.Range(-size.y, size.y), center.z + Random.Range(-size.z, size.z));

            NavMesh.SamplePosition(newPos, out navHit, 1000, NavMesh.AllAreas);

            if (navHit.hit)
                newPos = navHit.position;
        }

        return newPos;
    }

    [Server] void Update()
    {
        if (Time.frameCount % updateIncrement == 0)
        {
            if (canSpawnAi && !exitSpawned && (currentStrength < strengthCapacity))
                SpawnAnAi();

            IncrementUpdate();
        }

        if (exitSpawned || dragonFight)
            return;


        if (bossSpawned == false)
        {
            if (strengthKilled >= 100)
                SpawnBoss();
        }
        else
        {
            currentBoss.IncrementUpdate();

            if (bossKilled)
                SpawnExit();
        }
    }
    void IncrementUpdate()
    {
        foreach (Ai_BasicController ai in aiControllers)
        {
            if (ai != null)
                ai.IncrementUpdate();
        }
    }
}
