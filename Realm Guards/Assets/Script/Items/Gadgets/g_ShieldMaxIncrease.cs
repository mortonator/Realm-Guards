using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_ShieldMaxIncrease")]
public class g_ShieldMaxIncrease : IGadget
{
    [SerializeField] float increaseAmmount;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.maxShield += increaseAmmount;
    }
}
