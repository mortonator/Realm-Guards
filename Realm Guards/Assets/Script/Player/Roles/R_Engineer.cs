using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Role/r_Engineer")]
public class R_Engineer : IRole
{
    public override string GetText_Info() => "Provides the team with an improved shield.";
    public override string GetText_Data() => "Starting shield starts at 20 instead of 10";
    public override void ApplyEffect(PlayerController p)
    {
        //shield
        p.data.maxShield += 10;
    }
}
