using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Role/r_Demolitionist")]
public class R_Demolitionist : IRole
{
    public override string GetText_Info() => "Provides the team with body armour.";
    public override string GetText_Data() => "Increases starting health by 20";
    public override void ApplyEffect(PlayerController p)
    {
        //health
        p.data.maxHealth += 20;
    }
}
