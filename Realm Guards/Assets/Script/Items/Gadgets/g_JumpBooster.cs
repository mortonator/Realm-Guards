using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_JumpBooster")]
public class g_JumpBooster : IGadget
{
    [SerializeField] float increasePercentage;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.jumpForce *= increasePercentage;
    }
}
