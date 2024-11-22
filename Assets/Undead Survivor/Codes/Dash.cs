using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Undead_Survivor.Codes;

public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;
    private TrailRenderer trailRenderer;

    Vector2 dir;
    int damage, distance;
    const int Fdamage = 1, Fdistance = 5;

    public float dashDuration = 0.1f;  // 대시 지속 시간 (짧게 설정)
    public float dashCooldown = 0.5f;  // 대시 쿨타임 (명중 실패 시)
    private bool canDash = true;
    private Rigidbody2D rb;

    public GameObject hitEffectPrefab; // 히트 이펙트 프리팹

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
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

        Debug.Log("Setting up dash: damage = " + damage + ", distance = " + distance);
    }

    protected IEnumerator Dashing()
    {
        //canDash = false;  // 대시를 시작하면 임시로 대시 불가

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dir.normalized * distance;

        float elapsedTime = 0;
        bool hitTarget = false;

        // 트레일 렌더러 활성화
        if (trailRenderer != null)
            trailRenderer.enabled = true;

        // 데미지를 준 아이 저장
        HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / dashDuration);  // 비율을 0에서 1 사이로 고정
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
            if (hit2.CompareTag("Enemy"))
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
        }

        // 트레일 렌더러 비활성화
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        rb.position = targetPosition;

        if (hitTarget)
        {
            canDash = true;
        }
        else
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }

        OnDashEnd?.Invoke();  // 대시가 끝났음을 알림
    }

    protected void Effect()
    {
        //AudioManager.instance.PlaySfx(AudioManager.Sfx.dash);
        // 대시 시 비주얼 이펙트 (잔상, 이펙트 등) 처리
        Debug.Log("Dash effect triggered");
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            // 히트 이펙트 인스턴스화
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
        // Particle System이 있는 경우, 남은 시간 확인
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            // 파티클 시스템 재생이 완료될 때까지 대기
            yield return new WaitForSeconds(particle.main.duration);
        }
        else
        {
            // 애니메이터가 있는 경우
            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                // 애니메이션 클립 길이를 가져와 대기
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    yield return new WaitForSeconds(clipInfo[0].clip.length);
                }
            }
            else
            {
                // 애니메이션이나 파티클 시스템이 없는 경우 기본 시간 대기
                yield return new WaitForSeconds(1.0f);
            }
        }

        // 이펙트 오브젝트 삭제
        Destroy(effect);
    }
}
