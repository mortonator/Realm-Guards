using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GadgetChest : GadgetBox
{
    [Space]
    [SerializeField] GadgetChest_Half leftHalf;
    [SerializeField] GadgetChest_Half rightHalf;

    [HideInInspector, SyncVar] public int gadget2;

    public override string GetInteractText()
    {
        if (gadget == -1)
            return string.Empty;
        else if (isOpen)
            return GameManager.gadgetPrefabs[gadget].interactText;
        else
            return interactText;
    }

    public override void Interact(PlayerController p)
    {
        Debug.Log("Interact. " + isOpen + " : " + gadget);

        p.manager.Cmd_OpenChest(netId);
    }
    public void InteractHalf(PlayerController p, bool isLeft)
    {
        if (isLeft)
            GameManager.gadgetPrefabs[gadget].ApplyEffect(p);
        else
            GameManager.gadgetPrefabs[gadget2].ApplyEffect(p);

        p.manager.Cmd_CloseChest(netId, isLeft);
    }

    [ClientRpc] public override void Rpc_OpenChest()
    {
        Debug.Log("Open chest");
        isOpen = true;
        anim.SetTrigger("Open");
        snd_start.Play();

        leftHalf.gameObject.SetActive(true);
        rightHalf.gameObject.SetActive(true);
        boxCollider.enabled = false;

        leftHalf.interactText = GameManager.gadgetPrefabs[gadget].interactText;
        rightHalf.interactText = GameManager.gadgetPrefabs[gadget2].interactText;

        leftHalf.OpenHalf(gadget);
        rightHalf.OpenHalf(gadget2);
    }

    [ClientRpc] public override void Rpc_CloseChest(bool isLeft)
    {
        Debug.Log("Close chest");
        snd_end.Play();

        if (isLeft)
        {
            gadget = -1;
            leftHalf.CloseHalf();
        }
        else
        {
            gadget2 = -1;
            rightHalf.CloseHalf();
        }
    }
}
