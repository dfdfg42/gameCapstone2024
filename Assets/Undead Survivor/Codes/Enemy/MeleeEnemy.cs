using Assets.Undead_Survivor.Codes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("근접 공격 설정")]
    public float attackRange = 1.0f;
    public float attackCooldown = 1.0f;
    private float lastAttackTime;

    protected override void Move()
    {
        if (target == null) return;

        Vector2 dirVec = (target.position - transform.position).normalized;
        Vector2 nextVec = dirVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        // 공격 범위 체크
        float distance = Vector2.Distance(target.position, transform.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");

        // 데미지 처리
        if (GameManager.Instance.player != null)
        {
            IObjectDameged damageable = GameManager.Instance.player.GetComponent<IObjectDameged>();
            damageable?.Dameged(damage);
        }
    }
}

