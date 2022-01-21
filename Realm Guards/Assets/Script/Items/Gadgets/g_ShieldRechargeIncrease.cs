using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_ShieldRechargeIncrease")]
public class g_ShieldRechargeIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.shieldRecoveryMultiplier *= increaseMultiplier;
    }
}
