using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(AudioSource))]
public class ProjectileController : NetworkBehaviour
{
    [SerializeField] LayerMask hitMask;
    [SerializeField] float speed;
    [SerializeField] GameObject explosion;

    public float distance;
    float damage;
    public void SetDamage(float _damage) => damage = _damage;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, transform.forward * distance);
    }

    [Server] public static void Spawn(ProjectileController projectile, Vector3 pos, Quaternion rot, float _damage)
    {
        ProjectileController obj = Instantiate(projectile, pos, rot);
        obj.SetDamage(_damage);
        NetworkServer.Spawn(obj.gameObject);
    }

    [Server] void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        CheckCollision();
    }

    [Server] void CheckCollision()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, distance, hitMask))
        {
            if (hit.collider.TryGetComponent(out IDamagable dam))
                dam.Damage(damage);

            if (explosion != null)
                NetworkServer.Spawn(Instantiate(explosion, transform.position, transform.rotation));

            DoCollide();
            Destroy(gameObject, 0.5f);
        }
    }

    [ClientRpc] void DoCollide()
    {
        GetComponent<AudioSource>().Play();
        Destroy(gameObject, 0.5f);
    }
}
