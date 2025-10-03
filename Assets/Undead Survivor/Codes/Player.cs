using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Undead_Survivor.Codes;

public class Player : MonoBehaviour, IEffectTarget
{
    [Header("기본 스탯")]
    public float baseDamage = 10f;
    public float baseSpeed = 3f;
    public float baseDefense = 0f;

    [Header("현재 스탯 (읽기 전용)")]
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentDefense;

    [Header("기존 Player 변수들")]
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Dash dashComponent;

    private bool isDashing = false;
    private bool isImmune = false;
    private float immunityEndTime = 0f;

    // ⭐ 효과 캐시 (성능 최적화용)
    private List<IPassiveEffect> cachedPassiveEffects = new List<IPassiveEffect>();
    private List<IActiveEffect> cachedActiveEffects = new List<IActiveEffect>();

    // Dash 애니메이션을 위한 파라미터
    private readonly string ATTACK_VERTICAL = "AttackVertical";
    private readonly string ATTACK_HORIZONTAL = "AttackHorizontal";

    private float damageInterval = 0.5f;
    private float lastDamageTime;
    private HashSet<Collider2D> collidingEnemies = new HashSet<Collider2D>();

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        dashComponent = GetComponent<Dash>();

        // 기본 스탯으로 초기화
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentDefense = baseDefense;
        speed = currentSpeed;

        // Dash 컴포넌트에서 이벤트 구독
        if (dashComponent != null)
        {
            dashComponent.OnDashEnd += HandleDashEnd;
            dashComponent.OnHitTarget += OnDashHitTarget;
        }
    }

    void OnEnable()
    {
        // 기본 스탯으로 초기화
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentDefense = baseDefense;
        speed = currentSpeed;

        anim.runtimeAnimatorController = animCon[GameManager.Instance.playerId];
    }

    void Update()
    {
        if (isImmune && Time.time >= immunityEndTime)
        {
            isImmune = false;
            Debug.Log("Player is no longer immune.");
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive || isDashing)
            return;

        Vector2 nextVec = inputVec * currentSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void OnJump()
    {
        if (!isDashing)
        {
            OnDash();
        }
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    // ========== IEffectTarget 구현 ==========
    public void ApplyEffect(IEffect effect)
    {
        // 캐시 업데이트
        if (effect is IPassiveEffect passiveEffect)
        {
            cachedPassiveEffects.Add(passiveEffect);
            RecalculateStats(); // 패시브만 스탯 재계산
        }

        if (effect is IActiveEffect activeEffect)
        {
            cachedActiveEffects.Add(activeEffect);
        }

        effect.Apply(this); // 효과의 Apply 메서드 호출
        Debug.Log($"플레이어: {effect.Name} 효과 적용됨");
    }

    public void RemoveEffect(IEffect effect)
    {
        // 캐시에서 제거
        if (effect is IPassiveEffect passiveEffect)
        {
            cachedPassiveEffects.Remove(passiveEffect);
            RecalculateStats();
        }

        if (effect is IActiveEffect activeEffect)
        {
            cachedActiveEffects.Remove(activeEffect);
        }

        effect.Remove(this); // 효과의 Remove 메서드 호출
        Debug.Log($"플레이어: {effect.Name} 효과 제거됨");
    }

    public void TriggerActiveEffects(EffectTrigger trigger, object context = null)
    {
        var triggerableEffects = cachedActiveEffects.Where(e => e.CanTrigger(trigger)).ToList();

        if (triggerableEffects.Count > 0)
        {
            Debug.Log($"트리거 발동: {trigger}, 효과 개수: {triggerableEffects.Count}");

            foreach (var effect in triggerableEffects)
            {
                effect.ExecuteEffect(this, context);
            }
        }
    }

    // ========== 스탯 관리 ==========
    private void RecalculateStats()
    {
        // 기본값으로 초기화
        float damageMultiplier = 1f;
        float speedMultiplier = 1f;
        float defenseBonus = 0f;

        // 패시브 효과들만 스탯에 반영
        foreach (var passiveEffect in cachedPassiveEffects)
        {
            damageMultiplier += passiveEffect.GetStatModifier(StatType.Damage);
            speedMultiplier += passiveEffect.GetStatModifier(StatType.Speed);
            defenseBonus += passiveEffect.GetStatModifier(StatType.Defense);
        }

        // 최종 스탯 계산
        currentDamage = baseDamage * damageMultiplier;
        currentSpeed = baseSpeed * speedMultiplier;
        currentDefense = baseDefense + defenseBonus;

        // 기존 시스템과의 호환성
        speed = currentSpeed;

        Debug.Log($"스탯 업데이트 - 데미지: {currentDamage:F1}, 속도: {currentSpeed:F1}, 방어력: {currentDefense:F1}");
    }

    // ========== 대쉬 관련 ==========
    void OnDash()
    {
        Vector2 direction = inputVec.normalized;
        isDashing = true;

        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            anim.SetBool(ATTACK_VERTICAL, true);
            anim.SetBool(ATTACK_HORIZONTAL, false);
        }
        else
        {
            anim.SetBool(ATTACK_HORIZONTAL, true);
            anim.SetBool(ATTACK_VERTICAL, false);
        }

        dashComponent.Init(direction);
    }

    void HandleDashEnd()
    {
        isDashing = false;
        anim.SetBool(ATTACK_VERTICAL, false);
        anim.SetBool(ATTACK_HORIZONTAL, false);
    }

    private void OnDashHitTarget(Enemy target)
    {
        // 면역 효과 활성화
        isImmune = true;
        immunityEndTime = Time.time + 1.0f;
        Debug.Log("Player is immune until " + immunityEndTime);

        // 액티브 효과들 트리거
        if (target != null)
        {
            TriggerActiveEffects(EffectTrigger.DashHit, target);
        }
        else
        {
            Debug.Log("대쉬 완료 - 적 타격 없음");
        }
    }

    // ========== 충돌 및 피해 처리 ==========
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.isLive || isImmune || isDashing)
            return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null && !collidingEnemies.Contains(collision.collider))
        {
            collidingEnemies.Add(collision.collider);
            if (!isImmune)
            {
                StartCoroutine(ApplyDamageRoutine());
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            collidingEnemies.Remove(collision.collider);

            if (collidingEnemies.Count == 0)
            {
                anim.ResetTrigger("Damaged");
            }
        }
    }

    IEnumerator ApplyDamageRoutine()
    {
        while (GameManager.Instance.isLive && !isImmune && !isDashing && collidingEnemies.Count > 0)
        {
            float currentTime = Time.time;
            if (currentTime >= lastDamageTime + damageInterval)
            {
                float incomingDamage = 10f;
                float actualDamage = Mathf.Max(1f, incomingDamage - currentDefense);

                GameManager.Instance.health -= actualDamage;
                lastDamageTime = currentTime;

                // 피해받을 때 트리거 발동
                TriggerActiveEffects(EffectTrigger.PlayerDamaged, actualDamage);

                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Soldier_Hurt"))
                {
                    AudioManager.instance.PlaySfx(AudioManager.Sfx.PlayerHit);
                    anim.SetTrigger("Damaged");
                    StartCoroutine(ResetDamageTrigger());
                }

                if (GameManager.Instance.health <= 0)
                {
                    onDeath();
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ResetDamageTrigger()
    {
        yield return new WaitForSeconds(0.5f);
        anim.ResetTrigger("Damaged");
    }

    public void onDeath()
    {
        collidingEnemies.Clear();

        for (int index = 2; index < transform.childCount; index++)
        {
            transform.GetChild(index).gameObject.SetActive(false);
        }

        anim.SetTrigger("Dead");
        GameManager.Instance.GameOver();
    }

    // ========== 공개 접근자 ==========
    public float GetCurrentDamage() => currentDamage;
    public float GetCurrentSpeed() => currentSpeed;
    public float GetCurrentDefense() => currentDefense;

    // 효과 상태 확인 (디버그용)
    public int GetPassiveEffectCount() => cachedPassiveEffects.Count;
    public int GetActiveEffectCount() => cachedActiveEffects.Count;
}