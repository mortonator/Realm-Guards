using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_MagSizeIncrease")]
public class g_MagSizeIncrease : IGadget
{
    [SerializeField] int magSizeIncrease;

    public override void ApplyEffect(PlayerController p)
    {
        p.gun.IncreaseAmmoMax(magSizeIncrease);
    }
}
