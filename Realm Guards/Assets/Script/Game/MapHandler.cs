using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class MapHandler : MonoBehaviour
{
    [SerializeField, Scene] string[] mapNames;
    [SerializeField, Scene] string dragonMapName;
    [Scene] public string officeMapName;

    public string currentMapName { get; protected set; }
    int currentMapIndex;
    int completeMapIndex;
    int[] mapOrder;
    public static int currentMapNumber;

    public void BuildMapList()
    {
        GenerateMapList();
        //GenerateMapListInOrder();

        DragonManager.dragonIndex = -1;
        currentMapIndex = -1;
        completeMapIndex = -1;
        currentMapName = "ERROR: map only built. Call \"GetNextMap()\" first";
    }
    void GenerateMapList()
    {
        int length = mapNames.Length;
        mapOrder = new int[length];
        int rand;
        int attempt;

        for (int i = length - 1; i >= 0; i--)
            mapOrder[i] = -1;

        string debugMapOrder = "";
        for (int i = 0; i < length; i++)
        {
            attempt = 0;
            do
            {
                attempt++;
                if (attempt > 100)
                {
                    Debug.LogError("Map list generation error. Current list: " + debugMapOrder);
                    return;
                }

                rand = Random.Range(0, length);
            } while (mapOrder.Contains(rand));

            mapOrder[i] = rand;
            debugMapOrder += ", " + rand;
        }

        Debug.Log("Successfully generated map list: " + debugMapOrder);
    }
    /*void GenerateMapListInOrder() // TODO:: Remove this. Use GenerateMapList normally
    {
        mapOrder = new int[mapNames.Length];

        for (int i = mapOrder.Length - 1; i >= 0; i--)
            mapOrder[i] = i;
    }*/

    public void NextMap()
    {
        currentMapIndex++;
        completeMapIndex++;

        /*
        if (completeMapIndex > 0)
        {
            DragonManager.dragonIndex++;
            currentMapName = dragonMapName;
        }
        else
        {
            currentMapName = mapNames[mapOrder[currentMapIndex]];
        }
        */

        ///*
        if (completeMapIndex % 5 == 4)
        {
            DragonManager.dragonIndex++;
            currentMapName = dragonMapName;
        }
        else
        {
            if (currentMapIndex == mapOrder.Length - 1)
            {
                GenerateMapList();
                currentMapIndex = 0;
            }

            currentMapName = mapNames[mapOrder[currentMapIndex]];
        }
        //*/
    }
}
