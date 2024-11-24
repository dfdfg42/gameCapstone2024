using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;
<<<<<<< Updated upstream

    Vector2 dir;
    int damage, distance;
    const int Fdamage = 1, Fdistance = 5;

    public float dashDuration = 0.1f;  // 대시 지속 시간 (짧게 설정)
    public float dashCooldown = 0.5f;  // 대시 쿨타임 (명중 실패 시)
    private bool canDash = true;
    private Rigidbody2D rb;
=======
    private TrailRenderer trailRenderer;
    private Rigidbody2D rb;
    public GameObject hitEffectPrefab; // 히트 이펙트 프리팹

    const float Fdamage = 1, Fdistance = 5;
    public const float dashDuration = 0.1f;  // 대시 지속 시간 (짧게 설정)
    public const float dashCooldown = 0.5f;  // 대시 쿨타임 (명중 실패 시)

    private bool canDash = true;
    Vector2 dir;
    float damage, distance;
>>>>>>> Stashed changes

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        foreach (var upgrade in GameManager.upgrades) {
            SettingUpgrade(upgrade.Key, upgrade.Value);
        }

        Debug.Log("Setting up dash: damage = " + damage + ", distance = " + distance);
    }

    protected IEnumerator Dashing()
    {
        canDash = false;  // 대시를 시작하면 임시로 대시 불가

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dir.normalized * distance;

        float elapsedTime = 0;
        bool hitTarget = false;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / dashDuration);  // 비율을 0에서 1 사이로 고정
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);
            rb.MovePosition(newPosition);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Debug.Log("Dash hit: " + hit.name);
                    hitTarget = true;
                    break;
                }
            }

            yield return null;
        }

        rb.MovePosition(targetPosition);

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
<<<<<<< Updated upstream
        // 대시 시 비주얼 이펙트 (잔상, 이펙트 등) 처리
        Debug.Log("Dash effect triggered");
    }
=======

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
>>>>>>> Stashed changes
}
