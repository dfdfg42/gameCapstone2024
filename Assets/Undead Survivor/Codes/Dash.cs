using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Undead_Survivor.Codes;

public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;

    public delegate void HitTargetHandler();
    public event HitTargetHandler OnHitTarget;  // 적을 맞췄을 때 발생하는 이벤트 추가

    private TrailRenderer trailRenderer;

    Vector2 dir;
    float damage, distance;
    const float Fdamage = 1, Fdistance = 5;

    public float dashDuration = 0.1f;  // 대시 지속 시간
    public float dashCooldown = 0.5f;  // 대시 쿨타임
    private bool canDash = true;
    private Rigidbody2D rb;

    public GameObject hitEffectPrefab; // 히트 이펙트 프리팹

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
            trailRenderer.enabled = false; // 초기에는 비활성화
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
        damage = Fdamage;
        distance = Fdistance;

        foreach (var upgrade in GameManager.upgrades)
        {
            SettingUpgrade(upgrade.Key, upgrade.Value);
        }

    }

    protected IEnumerator Dashing()
    {
        canDash = false;  // 대시 중에는 대시 불가

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dir.normalized * distance;

        float elapsedTime = 0;
        bool hitTarget = false;

        // 트레일 렌더러 활성화
        if (trailRenderer != null)
            trailRenderer.enabled = true;

        // 데미지를 준 적 저장
        HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / dashDuration);
            rb.position = Vector2.Lerp(startPosition, targetPosition, t);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && !damagedTargets.Contains(hit))
                {
                    IObjectDameged target = hit.GetComponent<IObjectDameged>();
                    if (target != null)
                    {
                        damagedTargets.Add(hit);
                    }
                }
            }

            yield return null;
        }

        foreach (var hit2 in damagedTargets)
        {
            IObjectDameged target = hit2.GetComponent<IObjectDameged>();
            if (target != null)
            {
                target.Dameged(damage);
                hitTarget = true;

                // 히트 위치에 이펙트 생성
                SpawnHitEffect(hit2.transform.position);
            }
        }

        // 트레일 렌더러 비활성화
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        rb.position = targetPosition;

        if (hitTarget)
        {
            OnHitTarget?.Invoke();  // 적을 맞췄을 때 이벤트 발생
        }
        else
        {
            yield return new WaitForSeconds(dashCooldown);
        }

        canDash = true;  // 대시 가능 상태로 변경
        OnDashEnd?.Invoke();  // 대시 종료 이벤트 호출
    }

    protected void Effect()
    {
        // 대시 시 비주얼 이펙트 처리
        Debug.Log("Dash effect triggered");
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            // 히트 이펙트 생성
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

            // 이펙트 삭제 코루틴 시작
            StartCoroutine(DestroyAfterEffect(effect));
        }
        else
        {
            Debug.LogWarning("Hit effect prefab is not assigned!");
        }
    }

    private IEnumerator DestroyAfterEffect(GameObject effect)
    {
        // 이펙트의 재생 시간 동안 대기
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

        // 이펙트 오브젝트 삭제
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

}
