using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Undead_Survivor.Codes;

public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;

    // ⭐ 수정된 델리게이트: 타격한 적을 매개변수로 전달
    public delegate void HitTargetHandler(Enemy target);
    public event HitTargetHandler OnHitTarget;

    private TrailRenderer trailRenderer;
    private Player player;

    Vector2 dir;
    float damage, distance;
    const float Fdamage = 1, Fdistance = 5;

    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f;
    private bool canDash = true;
    private Rigidbody2D rb;

    public GameObject hitEffectPrefab;

    public ParticleSystem dashParticle; // ⭐ 대쉬 중에 재생할 파티클
    private AfterImagePool afterImagePool; // ⭐ 잔상 효과를 위한 참조
    private int originalLayer; // ⭐ 플레이어의 원래 레이어를 저장할 변수

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        player = GetComponent<Player>();
        afterImagePool = GetComponent<AfterImagePool>(); // ⭐ AfterImagePool 컴포넌트 가져오기

        originalLayer = gameObject.layer;

        if (trailRenderer != null)
            trailRenderer.enabled = false;
    }

    public void Init(Vector2 direction)
    {
        if (canDash)
        {
            this.dir = direction;
            Setting();
            StartCoroutine(Dashing());
            //Effect();
        }
    }

    protected void Setting()
    {
        // 플레이어의 현재 데미지 사용
        damage = player != null ? player.GetCurrentDamage() : Fdamage;
        distance = Fdistance;

        // 기존 업그레이드 시스템과의 호환성 유지
        foreach (var upgrade in GameManager.upgrades)
        {
            SettingUpgrade(upgrade.Key, upgrade.Value);
        }
    }

    protected IEnumerator Dashing()
    {
        // --- 1. 대쉬 준비 ---
        canDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; // 대쉬 중 중력 비활성화

        // ⭐ 플레이어 레이어를 DashingPlayer로 변경하여 적과 충돌하지 않게 함
        gameObject.layer = LayerMask.NameToLayer("DashingPlayer");

        // --- 2. 대쉬 시작 및 효과 재생 ---
        float dashSpeed = distance / dashDuration;
        rb.velocity = dir.normalized * dashSpeed; // 물리 속도로 대쉬 실행

        if (afterImagePool != null) afterImagePool.StartEffect();
        if (dashParticle != null) dashParticle.Play();
        if (trailRenderer != null) trailRenderer.enabled = true;

        // --- 3. 대쉬 시간 동안 적 타격 감지 ---
        float elapsedTime = 0;
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>(); // 중복 타격 방지를 위해 HashSet 사용

        while (elapsedTime < dashDuration)
        {
            // 타격 판정은 계속 수행
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null && !hitEnemies.Contains(enemy))
                    {
                        hitEnemies.Add(enemy);
                    }
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // --- 4. 대쉬 종료 및 효과 정지 ---
        rb.gravityScale = originalGravity; // 중력 복원
        rb.velocity = Vector2.zero; // 속도 초기화

        // ⭐ 플레이어 레이어를 원래대로 복원하여 다시 충돌하게 함
        gameObject.layer = originalLayer;


        if (afterImagePool != null) afterImagePool.StopEffect();
        if (dashParticle != null) dashParticle.Stop();
        if (trailRenderer != null) trailRenderer.enabled = false;

        // --- 5. 타격한 적들에게 데미지 및 효과 적용 ---
        foreach (var enemy in hitEnemies)
        {
            if (enemy != null)
            {
                enemy.Dameged(damage);
                SpawnHitEffect(enemy.transform.position);
                OnHitTarget?.Invoke(enemy);
            }
        }

        // 적을 한 명도 못 맞혔을 경우
        if (hitEnemies.Count == 0)
        {
            OnHitTarget?.Invoke(null);
        }

        // --- 6. 쿨다운 적용 (수정된 부분) ---
        // ⭐ 적을 못 맞혔을 때만 쿨다운을 적용합니다.
        if (hitEnemies.Count == 0)
        {
            yield return new WaitForSeconds(dashCooldown);
        }

        
        canDash = true;
        OnDashEnd?.Invoke();
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            StartCoroutine(DestroyAfterEffect(effect));
        }
    }

    private IEnumerator DestroyAfterEffect(GameObject effect)
    {
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            yield return new WaitForSeconds(particle.main.duration);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }
        Destroy(effect);
    }
    protected void SettingUpgrade(int utype, float uvalue)
    {
        switch (utype)
        {
            case 0:
                damage *= uvalue / 100 + 1;
                break;
            case 2:
                distance *= uvalue / 100 + 1;
                break;
        }
    }

    // 공개 메서드들
    public bool CanDash()
    {
        return canDash;
    }

    public float GetDashCooldown()
    {
        return dashCooldown;
    }

    public float GetDashDistance()
    {
        return distance;
    }
}