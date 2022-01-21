using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_ShieldRechargeTimerDecrease")]
public class g_ShieldRechargeTimerDecrease : IGadget
{
    [SerializeField] float decreaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.shieldRecoveryTimerSet *= decreaseMultiplier;
    }
}
