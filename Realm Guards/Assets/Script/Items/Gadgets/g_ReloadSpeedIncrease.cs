using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_ReloadSpeedIncrease")]
public class g_ReloadSpeedIncrease : IGadget
{
    [SerializeField] float increaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.gun.reloadSpeed *= increaseMultiplier;
    }
}
