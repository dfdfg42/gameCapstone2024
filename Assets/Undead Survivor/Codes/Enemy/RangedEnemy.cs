using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("원거리 공격 설정")]
    public float preferredDistance = 3f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int bulletPoolIndex = 3;

    private float lastAttackTime;
    private bool isAttacking;

    protected override void Move()
    {
        if (target == null || isAttacking) return;

        Vector2 dirVec = target.position - transform.position;
        float distance = dirVec.magnitude;
        dirVec.Normalize();

        Vector2 moveVec = Vector2.zero;

        if (distance < preferredDistance - 0.5f)
        {
            // 너무 가까우면 뒤로
            moveVec = -dirVec * speed;
        }
        else if (distance > preferredDistance + 0.5f)
        {
            // 너무 멀면 앞으로
            moveVec = dirVec * speed;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 적정 거리면 공격
            Attack();
            return;
        }

        if (moveVec != Vector2.zero)
        {
            Vector2 newPosition = rigid.position + moveVec * Time.fixedDeltaTime;
            rigid.MovePosition(newPosition);
        }
    }

    protected override void Attack()
    {
        StartCoroutine(RangedAttackRoutine());
    }

    private IEnumerator RangedAttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.35f);

        if (target != null)
        {
            Vector2 dirVec = (target.position - transform.position).normalized;

            GameObject bullet = GameManager.Instance.pool.Get(bulletPoolIndex);
            bullet.transform.position = firePoint != null ? firePoint.position : transform.position;

            MonsterBullet monsterBullet = bullet.GetComponent<MonsterBullet>();
            if (monsterBullet != null)
            {
                monsterBullet.Init(damage, dirVec);
            }

            float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }

        isAttacking = false;
    }

    public override void Init(SpawnData data)
    {
        base.Init(data);
        maxHealth = (int)(data.health * 0.75f); // 원거리 적은 체력이 적음
        health = maxHealth;
    }
}