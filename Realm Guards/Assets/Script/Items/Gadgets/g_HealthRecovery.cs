using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadget/g_HealthRecovery")]
public class g_HealthRecovery : IGadget
{
    [SerializeField] float recoverPercentage;

    public override void ApplyEffect(PlayerController p)
    {
        p.currentHealth += (recoverPercentage * p.data.maxHealth);
    }
}
