public class PlayerData
{
    //jumping 
    public int jumpMax = 1;
    public float jumpForce = 0.1f;

    //health 
    public float maxHealth = 100;

    //shield
    public float maxShield = 10;
    public float shieldRecoveryMultiplier = 4;
    public float shieldRecoveryTimerSet = 6;

    //revial
    public float revival_CountIncrement = 0.04f;
    public float revival_CountDecrement = 0.2f;
    public float revival_Range = 9; // 0.9 => revivalFX scale 0.04
}