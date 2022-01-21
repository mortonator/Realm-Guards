using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Role/r_Commander")]
public class R_Commander : IRole
{
    public override string GetText_Info() => "Provides the team with improved loadouts.";
    public override string GetText_Data() => "Increase ammo mag capacity by 10";
    public override void ApplyEffect(PlayerController p)
    {
        //ammo
        p.gun.IncreaseAmmoMax(10);
    }
}
