using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : NetworkBehaviour, IInteractable
{
    [SerializeField] Animator anim;
    [SyncVar] bool isOpen;

    public string GetInteractText()
    {
        if (isOpen)
            return " to close door";
        else
            return " to open door";
    }

    public void Interact(PlayerController p)
    {
        isOpen = !isOpen;
        anim.SetBool("IsOpen", isOpen);
    }
}
