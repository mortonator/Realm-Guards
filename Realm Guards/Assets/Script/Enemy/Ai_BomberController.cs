using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Ai_BomberController : Ai_BasicController
{

    NavMeshAgent agent;
    float distanceMultiplier;
    float halfRange;

    void OnValidate()
    {
        if (rangeOfAttack < 2)
            rangeOfAttack = 2;
    }

    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = rangeOfAttack;
        agent.updatePosition = anim == null;

        halfRange = rangeOfAttack / 2;
        distanceMultiplier = (rangeOfAttack + 2) / 2;

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

        foreach (RaycastHit hit in Physics.SphereCastAll(transform.position, rangeOfAttack, transform.forward))
        {
            if (hit.collider.gameObject.TryGetComponent(out IDamagable dam))
            {
                if (hit.distance < halfRange)
                    dam.Damage(damage * (distanceMultiplier - hit.distance));
                else
                    dam.Damage(damage);
            }
        }
        c = StartCoroutine(DestroyObject());
    }
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(attackRechargeDuration);

        if (EnemyManager.prefabDeathExplosion == null)
            NetworkServer.Spawn(Instantiate(EnemyManager.prefabDeathExplosion, transform.position, Quaternion.identity));

        EnemyManager.Instance.currentStrength -= strength;
        EnemyManager.Instance.aiControllers.Remove(this);
        NetworkServer.Destroy(gameObject);
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
