using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_RevivalSpeedIncrease")]
public class g_RevivalSpeedIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.revival_CountIncrement *= increaseMultiplier;
    }
}
