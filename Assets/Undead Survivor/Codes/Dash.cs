using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Undead_Survivor.Codes;

public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        player = GetComponent<Player>();

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
            Effect();
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
        canDash = false;

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dir.normalized * distance;

        float elapsedTime = 0;
        List<Enemy> hitEnemies = new List<Enemy>();

        if (trailRenderer != null)
            trailRenderer.enabled = true;

        HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / dashDuration);
            rb.position = Vector2.Lerp(startPosition, targetPosition, t);

            // 충돌 검사
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && !damagedTargets.Contains(hit))
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        damagedTargets.Add(hit);
                        hitEnemies.Add(enemy);
                    }
                }
            }

            yield return null;
        }

        // 트레일 렌더러 비활성화
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        rb.position = targetPosition;

        // 적들에게 데미지 적용 및 특수 효과 처리
        bool hitAnyTarget = false;
        foreach (var enemy in hitEnemies)
        {
            if (enemy != null)
            {
                // 기본 데미지 적용
                enemy.Dameged(damage);
                hitAnyTarget = true;

                // 히트 이펙트 생성
                SpawnHitEffect(enemy.transform.position);

                // 플레이어의 공격 성공 이벤트 호출 (특수 효과 적용)
                if (player != null)
                {
                    player.OnAttackSuccess(enemy);
                }

                // 개별 적 타격 이벤트 발생
                OnHitTarget?.Invoke(enemy);
            }
        }

        if (hitAnyTarget)
        {
            // 전체 타격 성공 이벤트 (면역 효과 등)
            OnHitTarget?.Invoke(null);
        }
        else
        {
            yield return new WaitForSeconds(dashCooldown);
        }

        canDash = true;
        OnDashEnd?.Invoke();
    }

    protected void Effect()
    {
        Debug.Log("Dash effect triggered");
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            StartCoroutine(DestroyAfterEffect(effect));
        }
        else
        {
            Debug.LogWarning("Hit effect prefab is not assigned!");
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
            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    yield return new WaitForSeconds(clipInfo[0].clip.length);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
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