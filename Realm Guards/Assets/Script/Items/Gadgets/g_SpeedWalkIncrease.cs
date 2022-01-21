using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_SpeedWalkIncrease")]
public class g_SpeedWalkIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.anim.SetFloat("speed_Walk", p.anim.GetFloat("speed_Walk") * increaseMultiplier);
    }
}
