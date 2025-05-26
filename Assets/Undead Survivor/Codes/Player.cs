using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Player 클래스에 IEffectTarget 인터페이스 구현
public class Player : MonoBehaviour, IEffectTarget
{
    [Header("기본 스탯")]
    public Vector2 inputVec;
    public float baseSpeed = 3f;
    public float baseDamage = 10f;

    [Header("현재 스탯 (효과 적용됨)")]
    public float speed;
    public float damage;
    public float defense = 0f;

    [Header("기존 컴포넌트")]
    public Scanner scanner;
    public RuntimeAnimatorController[] animCon;

    // 기존 컴포넌트들
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Dash dashComponent;

    // 효과 시스템 관련
    private List<IEffect> activeEffects;
    private Dictionary<EffectType, float> effectMultipliers;

    // 대쉬 및 전투 관련
    private bool isDashing = false;
    private bool isImmune = false;
    private float immunityEndTime = 0f;
    private float damageInterval = 0.5f;
    private float lastDamageTime;
    private HashSet<Collider2D> collidingEnemies = new HashSet<Collider2D>();

    // 애니메이션 파라미터
    private readonly string ATTACK_VERTICAL = "AttackVertical";
    private readonly string ATTACK_HORIZONTAL = "AttackHorizontal";

    void Awake()
    {
        // 기존 컴포넌트 초기화
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        dashComponent = GetComponent<Dash>();

        // 효과 시스템 초기화
        activeEffects = new List<IEffect>();
        effectMultipliers = new Dictionary<EffectType, float>();
        InitializeEffectMultipliers();

        // 기본 스탯 설정
        speed = baseSpeed;
        damage = baseDamage;

        // Dash 컴포넌트 이벤트 구독 (수정된 부분)
        if (dashComponent != null)
        {
            dashComponent.OnDashEnd += HandleDashEnd;
            dashComponent.OnHitTarget += ActivateImmunity; // 파라미터 맞춤
        }
    }

    void Start()
    {
        // 효과 매니저에 플레이어 등록
        if (EffectManager.Instance != null)
        {
            Debug.Log("플레이어가 효과 시스템에 등록되었습니다.");
        }
    }

    // ========== IEffectTarget 인터페이스 구현 ==========
    public void ApplyEffect(IEffect effect)
    {
        if (activeEffects.Contains(effect))
            return;

        activeEffects.Add(effect);
        ApplyEffectToStats(effect);

        Debug.Log($"플레이어에게 효과 적용: {effect.Name}");
    }

    public void RemoveEffect(IEffect effect)
    {
        if (!activeEffects.Contains(effect))
            return;

        activeEffects.Remove(effect);
        RemoveEffectFromStats(effect);

        Debug.Log($"플레이어에서 효과 제거: {effect.Name}");
    }

    // ========== 효과 시스템 메서드 ==========
    private void InitializeEffectMultipliers()
    {
        effectMultipliers[EffectType.Damage] = 1f;
        effectMultipliers[EffectType.Speed] = 1f;
        effectMultipliers[EffectType.Defense] = 0f;
    }

    private void ApplyEffectToStats(IEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.Damage:
                if (effect is DamageEffect)
                {
                    effectMultipliers[EffectType.Damage] += 0.25f;
                    damage = baseDamage * effectMultipliers[EffectType.Damage];
                }
                break;

            case EffectType.Speed:
                if (effect is SpeedEffect)
                {
                    effectMultipliers[EffectType.Speed] += 0.3f;
                    speed = baseSpeed * effectMultipliers[EffectType.Speed];
                }
                break;

            case EffectType.Defense:
                if (effect is DefenseEffect)
                {
                    effectMultipliers[EffectType.Defense] += 3f;
                    defense = effectMultipliers[EffectType.Defense];
                }
                break;

            case EffectType.Special:
                ApplySpecialEffect(effect);
                break;
        }

        UpdateStatsDisplay();
    }

    private void RemoveEffectFromStats(IEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.Damage:
                effectMultipliers[EffectType.Damage] -= 0.25f;
                if (effectMultipliers[EffectType.Damage] < 1f) effectMultipliers[EffectType.Damage] = 1f;
                damage = baseDamage * effectMultipliers[EffectType.Damage];
                break;

            case EffectType.Speed:
                effectMultipliers[EffectType.Speed] -= 0.3f;
                if (effectMultipliers[EffectType.Speed] < 1f) effectMultipliers[EffectType.Speed] = 1f;
                speed = baseSpeed * effectMultipliers[EffectType.Speed];
                break;

            case EffectType.Defense:
                effectMultipliers[EffectType.Defense] -= 3f;
                if (effectMultipliers[EffectType.Defense] < 0f) effectMultipliers[EffectType.Defense] = 0f;
                defense = effectMultipliers[EffectType.Defense];
                break;

            case EffectType.Special:
                RemoveSpecialEffect(effect);
                break;
        }

        UpdateStatsDisplay();
    }

    private void ApplySpecialEffect(IEffect effect)
    {
        switch (effect.EffectId)
        {
            case "time_slow":
                StartCoroutine(EnableTimeSlowOnHit());
                break;

            case "poison":
                EnablePoisonAttack();
                break;

            case "fire":
                EnableFireAttack();
                break;

            case "ice":
                EnableIceAttack();
                break;
        }
    }

    private void RemoveSpecialEffect(IEffect effect)
    {
        switch (effect.EffectId)
        {
            case "time_slow":
                DisableTimeSlowOnHit();
                break;

            case "poison":
                DisablePoisonAttack();
                break;

            case "fire":
                DisableFireAttack();
                break;

            case "ice":
                DisableIceAttack();
                break;
        }
    }

    // ========== 특수 효과 구현 ==========
    private bool timeSlowEnabled = false;
    private bool poisonEnabled = false;
    private bool fireEnabled = false;
    private bool iceEnabled = false;

    private IEnumerator EnableTimeSlowOnHit()
    {
        timeSlowEnabled = true;
        yield return null;
    }

    private void DisableTimeSlowOnHit()
    {
        timeSlowEnabled = false;
    }

    private void EnablePoisonAttack()
    {
        poisonEnabled = true;
    }

    private void DisablePoisonAttack()
    {
        poisonEnabled = false;
    }

    private void EnableFireAttack()
    {
        fireEnabled = true;
    }

    private void DisableFireAttack()
    {
        fireEnabled = false;
    }

    private void EnableIceAttack()
    {
        iceEnabled = true;
    }

    private void DisableIceAttack()
    {
        iceEnabled = false;
    }

    // 공격 성공 시 호출되는 메서드 (대쉬 공격 성공 시)
    public void OnAttackSuccess(Enemy target)
    {
        // 시간 느려짐 효과
        if (timeSlowEnabled)
        {
            StartCoroutine(ApplyTimeSlowEffect());
        }

        // 독 효과
        if (poisonEnabled)
        {
            StartCoroutine(ApplyPoisonToTarget(target));
        }

        // 화염 효과
        if (fireEnabled)
        {
            ApplyFireDamage(target);
        }

        // 얼음 효과
        if (iceEnabled)
        {
            TryFreezeTarget(target);
        }
    }

    private IEnumerator ApplyTimeSlowEffect()
    {
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

    private IEnumerator ApplyPoisonToTarget(Enemy target)
    {
        float poisonDamage = 2f;
        int poisonTicks = 3;

        for (int i = 0; i < poisonTicks; i++)
        {
            if (target != null)
            {
                target.Dameged(poisonDamage);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void ApplyFireDamage(Enemy target)
    {
        if (target != null)
        {
            float fireDamage = damage * 0.5f;
            target.Dameged(fireDamage);
        }
    }

    private void TryFreezeTarget(Enemy target)
    {
        if (target != null && Random.Range(0f, 1f) < 0.3f)
        {
            StartCoroutine(FreezeTarget(target));
        }
    }

    private IEnumerator FreezeTarget(Enemy target)
    {
        if (target != null)
        {
            float originalSpeed = target.speed;
            target.speed = 0f;
            yield return new WaitForSeconds(2f);
            if (target != null)
                target.speed = originalSpeed;
        }
    }

    // ========== 스탯 UI 업데이트 ==========
    private void UpdateStatsDisplay()
    {
        Debug.Log($"플레이어 스탯 업데이트 - 데미지: {damage:F1}, 속도: {speed:F1}, 방어력: {defense:F1}");
    }

    // ========== 기존 Player 메서드들 (수정됨) ==========
    void OnEnable()
    {
        speed = baseSpeed * Character.Speed;
        damage = baseDamage * Character.Damage;
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

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
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

    // ========== 충돌 및 데미지 처리 (기존 로직 유지) ==========
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
                float incomingDamage = 10f - defense;
                if (incomingDamage < 1f) incomingDamage = 1f;

                GameManager.Instance.health -= incomingDamage;
                lastDamageTime = currentTime;

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

    // ========== 대쉬 관련 메서드 ==========
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

    // 수정된 ActivateImmunity 메서드 - 델리게이트 시그니처에 맞춤
    private void ActivateImmunity(Enemy target)
    {
        isImmune = true;
        immunityEndTime = Time.time + 1.0f;
        Debug.Log("Player is immune until " + immunityEndTime);

        // 특수 효과 적용 (적을 타격했을 때)
        if (target != null)
        {
            OnAttackSuccess(target);
        }
    }

    // ========== 공개 메서드 (다른 시스템에서 호출) ==========
    public float GetCurrentDamage()
    {
        return damage;
    }

    public float GetCurrentSpeed()
    {
        return speed;
    }

    public float GetCurrentDefense()
    {
        return defense;
    }

    public List<IEffect> GetActiveEffects()
    {
        return new List<IEffect>(activeEffects);
    }
}