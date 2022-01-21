using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_DamageIncrease")]
public class g_DamageIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.gun.damage *= increaseMultiplier;
    }
}
