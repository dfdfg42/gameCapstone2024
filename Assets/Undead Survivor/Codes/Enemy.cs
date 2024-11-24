using Assets.Undead_Survivor.Codes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour, IObjectDameged
{
    // 기본 정보
    public float speed;
    public float health;
    public float maxHealth;
    public float rangedDistance = 3f; // 공격 거리

    // 공격
    public float damage = 10f;
    public EnemyAttackType attackType = EnemyAttackType.melee;

    // 특수 공격
    public float attackDamage = 3f; // 특수 공격 데미지
    public float attackSpeed = 1f; // 특수 공격 속도

    // 트리거
    public bool attacked;
    public float attackMaxCooldown = 2f; // 단위는 초
    public float attackCooldown = 2f;
    public float attackRange = 1.0f; // 근접 공격 범위

    bool isLive;

    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    int bulletIndex = 3; // 오브젝트 풀 인덱스

    // 러시 공격을 위한 변수
    private Vector2 rushTargetPosition; // 플레이어를 처음 감지했을 때의 위치 저장
    public bool playerDetected = false; // 플레이어를 감지했는지 여부

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();

        //////////////테스트/////////////////////////
        ///
        rigid.gravityScale = 0;  // 중력 영향 제거
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;  // 회전 방지
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 충돌 감지 모드 설정
        rigid.drag = 10;  // 마찰력 증가로 미끄러짐 감소
        rigid.mass = 100;  // 질량 증가로 밀림 현상 감소
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        float distance = Vector2.Distance(target.position, rigid.position);
        rigid.velocity = Vector2.zero;

        // 플레이어 추적
        if (attackType == EnemyAttackType.ranged) // 원거리 공격 타입일 때
        {
            Vector2 moveVec = Vector2.zero;
            // 공격
            if (attacked == true)
            {
                return;
            }
            else if (distance <= rangedDistance + 0.1f)  // 여유 범위 추가
            {
                if (attackCooldown <= 0f)
                {
                    onAttack();
                }
                else
                {
                    // 플레이어와 가까우면 뒤로 이동
                    moveVec = -dirVec.normalized * speed;
                }
            }
            else if (distance > rangedDistance + 0.1f)
            {
                // 플레이어와 멀면 앞으로 이동
                moveVec = dirVec.normalized * speed;
            }
            if (moveVec != Vector2.zero)
            {
                Vector2 newPosition = rigid.position + moveVec * Time.fixedDeltaTime;
                rigid.MovePosition(newPosition);
            }
        }
        else if (attackType == EnemyAttackType.rush)
        {
            // 공격 중이 아니라면 이동
            if (!attacked)
            {
                Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
                rigid.MovePosition(rigid.position + nextVec);
            }

            // 공격 쿨다운이 0 이하이고, 공격 중이 아니며, 플레이어가 감지 범위 안에 있을 때 공격
            if (attackCooldown <= 0f && !attacked && distance <= rangedDistance)
            {
                rushTargetPosition = target.position; // 플레이어의 위치 저장
                onAttack();
            }
        }
        else if (attackType == EnemyAttackType.melee) // 기본 타입일 때
        {
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);

            // 공격 범위 안에 들어오면 공격 애니메이션 재생
            if (distance <= attackRange)
            {
                anim.SetTrigger("Attack");
                //
                if (GameManager.Instance.player != null)
                {
                    IObjectDameged damageable = GameManager.Instance.player.GetComponent<IObjectDameged>();
                    if (damageable != null)
                    {
                        damageable.Dameged((int)damage);
                    }
                }

            }
        }
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        if (!isLive)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
        if ((attackCooldown >= 0f) && (attacked == false))
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    void OnEnable()
    {
        target = GameManager.Instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
        playerDetected = false; // 감지 상태 초기화
    }

    public void onAttack()
    {
        if (attackType == EnemyAttackType.ranged)
        {
            // 원거리 공격
            if ((attacked == false) && (attackCooldown <= 0f))
            {
                attackCooldown = 3f;
                attacked = true;

                StartCoroutine(rangedAttack());

            }
        }
        else if (attackType == EnemyAttackType.rush)
        {
            // 러시 공격
            if ((attacked == false) && (attackCooldown <= 0f))
            {
                attackCooldown = 3f;
                attacked = true;
                StartCoroutine(rushAttack());
            }
        }
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        if (attackType == EnemyAttackType.melee)
        {
            maxHealth = data.health;
            health = maxHealth;
        }
        else if (attackType == EnemyAttackType.ranged)
        {
            maxHealth = (int)(data.health * 0.75);
            health = maxHealth;
        }
        else if (attackType == EnemyAttackType.rush)
        {
            maxHealth = (int)(data.health * 2);
            health = maxHealth;
        }

    }

    public void Dameged(float tempdamege)
    {
        health -= tempdamege;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.Instance.kill++;
            GameManager.Instance.GetExp();

            if (GameManager.Instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
            Dead();
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait; // 다음 물리 프레임까지 딜레이
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 1.3f, ForceMode2D.Impulse);
    }

    IEnumerator rushAttack()
    {
        // 공격 전 돌진 준비 모션
        anim.SetTrigger("Ready_Rush");

        yield return new WaitForSeconds(1.0f);

        anim.SetTrigger("Rush");
        // 돌진 로직
        Vector2 dirVec = rushTargetPosition - rigid.position; // 저장된 위치로 돌진
        rigid.velocity = Vector2.zero;

        // 방향을 조정하지 않고 저장된 위치로 이동
        for (int i = 0; i < 10; i++)
        {
            Vector2 nextVec = dirVec.normalized * speed * attackSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            yield return new WaitForSeconds(0.01f);
        }

        attacked = false;
        playerDetected = false; // 공격 후 감지 상태 초기화
    }

    IEnumerator rangedAttack()
    {
        // 공격 모션
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.35f);

        // 플레이어 방향 계산
        Vector2 dirVec = target.position - rigid.position;
        dirVec = dirVec.normalized;
        // 오브젝트 풀에서 총알 가져오기
        GameObject bullet = GameManager.Instance.pool.Get(bulletIndex);  // MonsterBullet의 풀 인덱스
        bullet.transform.position = transform.position;  // 몬스터의 위치에서 발사
        // 총알 초기화
        MonsterBullet monsterBullet = bullet.GetComponent<MonsterBullet>();
        if (monsterBullet != null)
        {
            monsterBullet.Init(attackDamage, dirVec);
        }
        // 플레이어 바라보도록 총알 회전
        float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        attacked = false;
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}

public enum EnemyAttackType
{
    ranged,
    melee,
    rush
}