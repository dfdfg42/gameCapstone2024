using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Dash : MonoBehaviour
{
    public delegate void DashEndHandler();
    public event DashEndHandler OnDashEnd;

    Vector2 dir;
    int damage, distance;
    const int Fdamage = 1, Fdistance = 5;

    public float dashDuration = 0.1f;  // 대시 지속 시간 (짧게 설정)
    public float dashCooldown = 0.5f;  // 대시 쿨타임 (명중 실패 시)
    private bool canDash = true;
    private Rigidbody2D rb;

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
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashDuration);
            rb.MovePosition(newPosition);

            elapsedTime += Time.deltaTime;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("enemy"))
                {
                    Debug.Log("Dash hit: " + hit.name);
                    hitTarget = true;
                    break;
                }
            }

            if (hitTarget)
                break;

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
        // 대시 시 비주얼 이펙트 (잔상, 이펙트 등) 처리
        Debug.Log("Dash effect triggered");
    }
}
