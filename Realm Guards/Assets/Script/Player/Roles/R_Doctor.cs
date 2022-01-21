using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Role/r_Doctor")]
public class R_Doctor : IRole
{
    public override string GetText_Info() => "Teachs the team basic first aid.";
    public override string GetText_Data() => "Revival speed increases by 10%";
    public override void ApplyEffect(PlayerController p)
    {
        //health
        p.data.revival_CountIncrement *= 1.1f;
    }
}
