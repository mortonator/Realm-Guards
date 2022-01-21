using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Ai_DragonController : Ai_BasicController
{
    [Header("Breath Attack")]
    [SerializeField] GameObject breathEffect;
    [SerializeField] float breathAttack_recharge = 20;
    [SerializeField] float breathAttack_range = 6;
    [SerializeField] float breathAttack_angle = 30;
    [SerializeField] float breathAttack_damage = 2;

    NavMeshAgent agent;
    bool canBreathAttack;
    float breathAttack_rangeHalf;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(breathEffect.transform.position, breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, (Quaternion.AngleAxis(breathAttack_angle, Vector3.up) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, (Quaternion.AngleAxis(-breathAttack_angle, Vector3.up) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, (Quaternion.AngleAxis(breathAttack_angle, Vector3.right) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, (Quaternion.AngleAxis(-breathAttack_angle, Vector3.right) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, ((Quaternion.AngleAxis(-breathAttack_angle * (3 / 4f), Vector3.right) * Quaternion.AngleAxis(-breathAttack_angle * (3 / 4f), Vector3.up)) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, ((Quaternion.AngleAxis(breathAttack_angle * (3 / 4f), Vector3.right) * Quaternion.AngleAxis(-breathAttack_angle * (3 / 4f), Vector3.up)) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, ((Quaternion.AngleAxis(-breathAttack_angle * (3 / 4f), Vector3.right) * Quaternion.AngleAxis(breathAttack_angle * (3 / 4f), Vector3.up)) * breathEffect.transform.forward) * breathAttack_range);
        Gizmos.DrawRay(breathEffect.transform.position, ((Quaternion.AngleAxis(breathAttack_angle * (3 / 4f), Vector3.right) * Quaternion.AngleAxis(breathAttack_angle * (3 / 4f), Vector3.up)) * breathEffect.transform.forward) * breathAttack_range);
    }

    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = rangeOfAttack;
        agent.updatePosition = anim == null;

        breathAttack_rangeHalf = breathAttack_range / 2;

        base.Start();
    }

    void Update()
    {
        if (playerTarget != null)
        {
            if (anim != null)
                MoveAnimViaAgent();

            if (canBreathAttack && (agent.remainingDistance <= breathAttack_rangeHalf))
                BreathAttack();
            else if (canAttack && (agent.remainingDistance <= rangeOfAttack))
                Attack();
        }

        if (Time.frameCount % EnemyManager.updateIncrement == 0)
            IncrementUpdate();
    }
    void MoveAnimViaAgent()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        if (velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius)
        {
            // Update animation parameters
            anim.SetFloat("Move Z", velocity.y);
        }
        else
        {
            anim.SetFloat("Move Z", 0);
        }
    }
    void OnAnimatorMove()
    {
        // Update position to agent position
        transform.position = agent.nextPosition;
    }

    public override void IncrementUpdate()
    {
        base.IncrementUpdate();

        if (agent != null && playerTarget != null)
            agent.SetDestination(playerTarget.position);
    }
    void Attack()
    {
        anim.SetTrigger("Attack");
        canAttack = false;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rangeOfAttack))
        {
            if (hit.collider.gameObject.TryGetComponent(out IDamagable dam))
            {
                dam.Damage(damage);
                c = StartCoroutine(RechargeAttack());
            }
        }
    }
    IEnumerator RechargeAttack()
    {
        yield return new WaitForSeconds(attackRechargeDuration);
        canAttack = true;
    }
    void BreathAttack()
    {
        float sqrRange = breathAttack_range * breathAttack_range;
        Vector3 dir;

        foreach (PlayerManager man in RG_NetworkManager.players)
        {
            dir = man.playerCon.transform.position - transform.position;

            if (dir.sqrMagnitude < sqrRange && Vector3.Angle(dir, breathEffect.transform.forward) < breathAttack_angle)
                man.playerCon.Damage(breathAttack_damage);
        }

        canBreathAttack = false;
        StartCoroutine(RechargeBreathAttack());
    }
    IEnumerator RechargeBreathAttack()
    {
        yield return new WaitForSeconds(breathAttack_recharge);
        canBreathAttack = true;
    }

    public override void HandleDamage(float oldValue, float newValue)
    {
        base.HandleDamage(oldValue, newValue);

        if (health > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            if (healthBar != null)
            {
                EnemyManager.Instance.bossKilled = true;
                EnemyManager.Instance.bossLocation = transform.position;
                EnemyManager.Instance.bossYAngle = transform.eulerAngles.y;
            }
            else if (netIdentity.isServer)
            {
                DragonManager.instance.dragonPosition = transform.position;
                DragonManager.instance.dragonYAngle = transform.eulerAngles.y;
                DragonManager.instance.DragonKilled();
            }

            anim.SetTrigger("Die");
            agent.isStopped = true;
            this.enabled = false;
            StartCoroutine(EndOFDeathAnim());
        }
    }
}
