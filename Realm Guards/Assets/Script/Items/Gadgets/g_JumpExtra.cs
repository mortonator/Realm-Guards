using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_JumpExtra")]
public class g_JumpExtra : IGadget
{
    public override void ApplyEffect(PlayerController p)
    {
        p.data.jumpMax++;
    }
}
