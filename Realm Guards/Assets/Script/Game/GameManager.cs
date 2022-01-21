using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Public")]
    public GameObject worldGameCanvas;
    public UnityEngine.Rendering.Volume volume;
    public UnityEngine.Audio.AudioMixer mainMixer;

    [Header("SerializeFields")]
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] bool spawnBoxes;
    [SerializeField] Gradient gadgetColourGradient;

    public static IGadget[] gadgetPrefabs;
    static GadgetBox[] boxPrefabs;
    static GadgetChest chestPrefabs;
    static AnimationCurve gadgetCurve;
    static int maxTime;

    void OnEnable()
    {
        Instance = this;

        if (IGadget.gradientValueColour == null)
            IGadget.gradientValueColour = gadgetColourGradient;
    }
    void OnDisable()
    {
        Instance = null;
    }

    [Server] void Start()
    {
        if (boxPrefabs == null)
        {
            boxPrefabs = Resources.LoadAll<GadgetBox>("SpawnablePrefabs/Boxes/");
            chestPrefabs = Resources.Load<GadgetChest>("SpawnablePrefabs/Boxes/ChestBox");
        }
        if (gadgetPrefabs == null)
        {
            gadgetPrefabs = Resources.LoadAll<IGadget>("Gadgets/");

            int currentTime = 0;
            gadgetCurve = new AnimationCurve();
            for (int i = 0; i < gadgetPrefabs.Length; i++)
            {
                gadgetCurve.AddKey(currentTime, i);
                currentTime += gadgetPrefabs[i].value;
                gadgetCurve.AddKey(currentTime, i);
            }
            maxTime = currentTime + 1;
        }

        Debug.Log(gadgetPrefabs.Length);

        if (spawnBoxes)
        {
            for (int i = 20; i > 0; i--)
                SpawnBox();
        }
    }

    [Server] void SpawnBox()
    {
        int rng = Random.Range(0, 14);
        int gadgetIndex, otherIndex;


        if (rng == 13)
        {
            GadgetChest chest = Instantiate(chestPrefabs, enemyManager.GetRandomPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0));

            chest.gadget = (int)gadgetCurve.Evaluate(Random.Range(0, maxTime));
            chest.gadget2 = (int)gadgetCurve.Evaluate(Random.Range(0, maxTime));

            NetworkServer.Spawn(chest.gameObject);
        }
        else if (rng > 11)
        {
            GadgetBox box = Instantiate(boxPrefabs[1], enemyManager.GetRandomPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0));
            gadgetIndex = Random.Range(0, maxTime);

            otherIndex = Random.Range(0, maxTime);
            if (gadgetIndex < otherIndex) gadgetIndex = otherIndex;

            otherIndex = Random.Range(0, maxTime);
            if (gadgetIndex < otherIndex) gadgetIndex = otherIndex;

            otherIndex = Random.Range(0, maxTime);
            if (gadgetIndex < otherIndex) gadgetIndex = otherIndex;

            box.gadget = (int)gadgetCurve.Evaluate(gadgetIndex);
            NetworkServer.Spawn(box.gameObject);
        }
        else if (rng > 6)
        {
            GadgetBox box = Instantiate(boxPrefabs[2], enemyManager.GetRandomPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0));
            gadgetIndex = Random.Range(0, maxTime);

            otherIndex = Random.Range(0, maxTime);
            if (gadgetIndex < otherIndex) gadgetIndex = otherIndex;

            box.gadget = (int)gadgetCurve.Evaluate(gadgetIndex);
            NetworkServer.Spawn(box.gameObject);
        }
        else
        {
            GadgetBox box = Instantiate(boxPrefabs[0], enemyManager.GetRandomPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0));
            gadgetIndex = Random.Range(0, maxTime);

            box.gadget = (int)gadgetCurve.Evaluate(gadgetIndex);
            NetworkServer.Spawn(box.gameObject);
        }
    }

    [Server] public void Ser_ApplyRoleEffects()
    {
        RPC_ApplyRoleEffects();
    }
    [ClientRpc] void RPC_ApplyRoleEffects()
    {
        foreach (PlayerManager player in RG_NetworkManager.players)
        {
            PlayerSelection.rolePrefabs[player.roleSelection].ApplyEffect(RG_NetworkManager.localPlayer.playerCon);
        }
    }
}
/*
 0, 1, 2, 3, 4, 5, 6,
 7, 8, 9,10,11
11,12
13
*/