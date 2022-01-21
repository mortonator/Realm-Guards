using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Ai_MeleeController : Ai_BasicController
{
    NavMeshAgent agent;

    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = rangeOfAttack;
        agent.updatePosition = anim == null;

        base.Start();
    }

    void Update()
    {
        if (playerTarget != null)
        {
            if (anim != null)
                MoveAnimViaAgent();

            if (canAttack && (agent.remainingDistance <= rangeOfAttack))
                Attack();
        }
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

        if (Physics.Raycast(transform.position+ transform.up, transform.forward, out RaycastHit hit, rangeOfAttack))
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
            else if (EnemyManager.Instance.aiControllers.Contains(this))
            {
                EnemyManager.Instance.currentStrength -= strength;
                EnemyManager.Instance.strengthKilled += strength;
                EnemyManager.Instance.aiControllers.Remove(this);
            }

            anim.SetTrigger("Die");
            agent.isStopped = true;
            this.enabled = false;
            StartCoroutine(EndOFDeathAnim());
        }
    }
}
