using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoHandler : MonoBehaviour
{
    [SerializeField] Animator anim;
    List<Collider> col = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (col.Count == 0)
            anim.SetTrigger("Open");

        col.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (col.Contains(other))
            col.Remove(other);

        if (col.Count == 0)
            anim.SetTrigger("Close");
    }
}
