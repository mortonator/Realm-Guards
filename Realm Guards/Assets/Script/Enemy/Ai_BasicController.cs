using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkTransform))]
public class Ai_BasicController : NetworkBehaviour, IDamagable
{
    [SerializeField] Ai_Info info;
    public Ai_Info Info => info;

    [Space]

    public int strength;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int damage;
    [SerializeField] protected float rangeOfAttack;
    [SerializeField] protected float attackRechargeDuration;
    [SerializeField] protected float attackHeight;
    [SerializeField] protected float rangeOfSight;

    protected Animator anim;
    protected Coroutine c;

    protected Transform playerTarget;

    protected Vector2 smoothDeltaPosition = Vector2.zero;
    protected Vector2 velocity = Vector2.zero;
    [SyncVar(hook = nameof(HandleDamage))] protected float health;
    protected bool canAttack = true;
    protected float deathAnimDuration;

    [HideInInspector] public UnityEngine.UI.Image healthBar;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position + (transform.up * attackHeight), transform.forward);
    }
    public virtual void Start()
    {
        anim = GetComponent<Animator>();

        health = maxHealth;
    }

    protected void FindTarget()
    {
        float currentLowestDistance = -1, nextDistance;
        foreach (PlayerManager man in RG_NetworkManager.players)
        {
            nextDistance = (man.playerCon.transform.position - transform.position).sqrMagnitude;
            if (nextDistance < currentLowestDistance || currentLowestDistance == -1)
            {
                currentLowestDistance = nextDistance;
                playerTarget = man.playerCon.transform;
            }
        }
    }

    public virtual void IncrementUpdate()
    {
        FindTarget();

        if (playerTarget == null)
            return;
    }
    public virtual void Damage(float dam)
    {
        health -= dam;
    }
    public virtual void HandleDamage(float oldValue, float newValue)
    { 
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }
    public IEnumerator EndOFDeathAnim()
    {
        info.SetToDefeated();

        yield return new WaitForSeconds(deathAnimDuration);

        if (EnemyManager.prefabDeathExplosion == null)
            NetworkServer.Spawn(Instantiate(EnemyManager.prefabDeathExplosion, transform.position, Quaternion.identity));

        NetworkServer.Destroy(gameObject);
    }
}
