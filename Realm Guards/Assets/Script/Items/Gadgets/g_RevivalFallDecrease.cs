using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_RevivalFallDecrease")]
public class g_RevivalFallDecrease : IGadget
{
    [SerializeField] float decreaseMultiplier;

    public override void ApplyEffect(PlayerController p)
    {
        p.data.revival_CountDecrement *= decreaseMultiplier;
    }
}
