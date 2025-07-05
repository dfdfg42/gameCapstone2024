using Assets.Undead_Survivor.Codes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushEnemy : Enemy
{
    [Header("돌진 공격 설정")]
    public float detectionRange = 5f;
    public float rushSpeed = 10f;
    public float rushDuration = 1f;
    public float rushCooldown = 3f;

    private bool isRushing;
    private Vector2 rushDirection;
    private float lastRushTime;

    protected override void Move()
    {
        if (target == null || isRushing) return;

        Vector2 dirVec = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(target.position, transform.position);

        if (distance <= detectionRange && Time.time >= lastRushTime + rushCooldown)
        {
            Attack();
        }
        else if (!isRushing)
        {
            // 일반 이동
            Vector2 nextVec = dirVec * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }
    }

    protected override void Attack()
    {
        StartCoroutine(RushAttackRoutine());
    }

    private IEnumerator RushAttackRoutine()
    {
        isRushing = true;
        lastRushTime = Time.time;

        // 준비 모션
        anim.SetTrigger("Ready_Rush");
        yield return new WaitForSeconds(0.5f);

        // 돌진 방향 설정
        rushDirection = (target.position - transform.position).normalized;
        anim.SetTrigger("Rush");

        // 돌진
        float rushTime = 0;
        while (rushTime < rushDuration)
        {
            Vector2 rushVec = rushDirection * rushSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + rushVec);
            rushTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isRushing = false;
    }

    public override void Init(SpawnData data)
    {
        base.Init(data);
        maxHealth = (int)(data.health * 2f); // 돌진 적은 체력이 많음
        health = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isRushing && collision.gameObject.CompareTag("Player"))
        {
            IObjectDameged damageable = collision.gameObject.GetComponent<IObjectDameged>();
            damageable?.Dameged(damage * 2f); // 돌진 데미지는 2배
        }
    }
}