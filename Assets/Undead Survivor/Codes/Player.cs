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

    // ⭐ 개선된 효과 시스템
    private List<IEffect> activeEffects = new List<IEffect>();
    private List<IPassiveEffect> passiveEffects = new List<IPassiveEffect>();
    private List<IActiveEffect> activeTriggeredEffects = new List<IActiveEffect>();

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
        RecalculateStats();

        // Dash 컴포넌트에서 이벤트 구독
        if (dashComponent != null)
        {
            dashComponent.OnDashEnd += HandleDashEnd;
            dashComponent.OnHitTarget += OnDashHitTarget;  // ⭐ 수정된 메서드명
        }
    }

    void OnEnable()
    {
        RecalculateStats();
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

    // ⭐ 개선된 IEffectTarget 구현
    public void ApplyEffect(IEffect effect)
    {
        // 기존 같은 ID 효과 제거
        RemoveEffect(effect);

        // 새 효과 추가
        activeEffects.Add(effect);

        // 효과 타입별로 분류
        if (effect is IPassiveEffect passiveEffect)
        {
            passiveEffects.Add(passiveEffect);
            Debug.Log($"패시브 효과 추가: {effect.Name}");
        }

        if (effect is IActiveEffect activeEffect)
        {
            activeTriggeredEffects.Add(activeEffect);
            Debug.Log($"액티브 효과 추가: {effect.Name}");
        }

        // 패시브 효과의 경우에만 스탯 재계산
        if (effect.Category == EffectCategory.Passive)
        {
            RecalculateStats();
        }

        Debug.Log($"플레이어: {effect.Name} 효과 적용됨");
    }

    public void RemoveEffect(IEffect effect)
    {
        // 모든 리스트에서 같은 ID 효과 제거
        activeEffects.RemoveAll(e => e.EffectId == effect.EffectId);
        passiveEffects.RemoveAll(e => e.EffectId == effect.EffectId);
        activeTriggeredEffects.RemoveAll(e => e.EffectId == effect.EffectId);

        // 패시브 효과였다면 스탯 재계산
        if (effect.Category == EffectCategory.Passive)
        {
            RecalculateStats();
        }

        Debug.Log($"플레이어: {effect.Name} 효과 제거됨");
    }

    // ⭐ 새로운 트리거 시스템
    public void TriggerActiveEffects(EffectTrigger trigger, object context = null)
    {
        var triggerableEffects = activeTriggeredEffects.Where(e => e.CanTrigger(trigger)).ToList();

        if (triggerableEffects.Count > 0)
        {
            Debug.Log($"트리거 발동: {trigger}, 효과 개수: {triggerableEffects.Count}");

            foreach (var effect in triggerableEffects)
            {
                effect.ExecuteEffect(this, context);
            }
        }
    }

    // ⭐ 개선된 스탯 계산 (패시브 효과만)
    private void RecalculateStats()
    {
        // 기본값으로 초기화
        float damageMultiplier = 1f;
        float speedMultiplier = 1f;
        float defenseBonus = 0f;

        // 패시브 효과들만 스탯에 반영
        foreach (var passiveEffect in passiveEffects)
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

    // ⭐ 대쉬로 적 타격 시 호출 (기존 OnAttackSuccess 대체)
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
            // target이 null이면 단순히 대쉬만 한 것 (적 타격 X)
            Debug.Log("대쉬 완료 - 적 타격 없음");
        }
    }

    // 공개 스탯 접근자들
    public float GetCurrentDamage() => currentDamage;
    public float GetCurrentSpeed() => currentSpeed;
    public float GetCurrentDefense() => currentDefense;
    public List<IEffect> GetActiveEffects() => new List<IEffect>(activeEffects);

    // ⭐ 효과 상태 확인 메서드들
    public bool HasPassiveEffect(string effectId)
    {
        return passiveEffects.Any(e => e.EffectId == effectId);
    }

    public bool HasActiveEffect(string effectId)
    {
        return activeTriggeredEffects.Any(e => e.EffectId == effectId);
    }

    public int GetActiveEffectCount(EffectCategory category)
    {
        return activeEffects.Count(e => e.Category == category);
    }

    // 기존 충돌 및 전투 로직 (변경 없음)
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
}