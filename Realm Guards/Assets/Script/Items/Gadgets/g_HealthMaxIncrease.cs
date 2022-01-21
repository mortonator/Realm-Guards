using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_HealthMaxIncrease")]
public class g_HealthMaxIncrease : IGadget
{
    [SerializeField] int increaseAmmount;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.maxHealth += increaseAmmount;
    }
}
