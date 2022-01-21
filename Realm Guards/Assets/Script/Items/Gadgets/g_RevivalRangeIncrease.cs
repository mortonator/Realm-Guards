using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_RevivalRangeIncrease")]
public class g_RevivalRangeIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.IncreaseRevivalCirclePercentage(increaseMultiplier);
    }
}
