/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeManager : MonoBehaviour
{
    public static OfficeManager Instance;

    [SerializeField] Transform readyParent;
    [SerializeField] ReadyBox readyBox;

    List<ReadyBox> readys;

    private void Awake()
    {
        if (Instance != null && Instance.gameObject != null)
            Destroy(Instance.gameObject);

        Instance = this;
        readys = new List<ReadyBox>();
    }


    public void AddReady(string nameStr)
    {
        Debug.Log("Add a Ready");

        readys.Add(
            Instantiate(readyBox, readyParent)
            );

        readys[readys.Count - 1].gameObject.SetActive(true);
        readys[readys.Count - 1].SetName(nameStr);
    }
    public void UpdateReady()
    {
        Debug.Log("Update Ready");

        int i=0;
        foreach (PlayerManager man in RG_NetworkManager.players)
        {
            if (i >= readys.Count)
            {
                Debug.LogWarning("Not enough Readys for each Player. Player count = " + RG_NetworkManager.players.Count + ". Ready count = " + readys.Count);
                return;
            }

            Debug.Log(man.displayName + " | " + man.isReady);
            readys[i].SetName(man.displayName);

            if (man.isReady)
                readys[i].ReadyUp();
            else
                readys[i].ReadyDown();

            i++;
        }
    }
    public void RemoveReady()
    {
        Debug.Log("Remove a Ready");

        readys.RemoveAt(readys.Count - 1);
        UpdateReady();
    }
}
*/