using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_SpeedSprintIncrease")]
public class g_SpeedSprintIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.anim.SetFloat("speed_Sprint", p.anim.GetFloat("speed_Sprint") * increaseMultiplier);
    }
}
