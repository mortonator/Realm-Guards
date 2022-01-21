using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Role/r_Marksman")]
public class R_Marksman : IRole
{
    public override string GetText_Info() => "Teachs the team how to identify weak points.";
    public override string GetText_Data() => "Damage increases by 10%";
    public override void ApplyEffect(PlayerController p)
    {
        //damage
        p.gun.damage *= 1.1f;
    }
}
