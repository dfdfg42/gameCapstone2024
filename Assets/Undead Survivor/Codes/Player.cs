using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Dash dashComponent;

    private bool isDashing = false;   // 대시 상태 플래그
    private bool isImmune = false;    // 데미지 면역 상태 플래그
    private float immunityEndTime = 0f;  // 면역 종료 시간

    // Dash 애니메이션을 위한 파라미터 추가
    private readonly string ATTACK_VERTICAL = "AttackVertical";
    private readonly string ATTACK_HORIZONTAL = "AttackHorizontal";

    private float damageInterval = 0.5f;  // 데미지를 받는 간격
    private float lastDamageTime;  // 마지막으로 데미지를 받은 시간
    private HashSet<Collider2D> collidingEnemies = new HashSet<Collider2D>();  // 현재 충돌 중인 적들

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        dashComponent = GetComponent<Dash>();

        // Dash 컴포넌트에서 이벤트 구독
        if (dashComponent != null)
        {
            dashComponent.OnDashEnd += HandleDashEnd;
            dashComponent.OnHitTarget += ActivateImmunity;  // 적을 맞췄을 때 면역 활성화
        }
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.Instance.playerId];
    }

    void Update()
    {
        // 면역 상태 관리
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
        if (!isDashing) // 대시 중이 아닐 때만 대시 가능
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

    // 충돌 시작 시 호출
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.isLive || isImmune || isDashing)
            return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null && !collidingEnemies.Contains(collision.collider))
        {
            collidingEnemies.Add(collision.collider);
            if (!isImmune)  // 면역 상태가 아닐 때만 데미지 루틴 시작
            {
                StartCoroutine(ApplyDamageRoutine());
            }
        }
    }

    // 충돌 종료 시 호출
    void OnCollisionExit2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            collidingEnemies.Remove(collision.collider);

            if (collidingEnemies.Count == 0)
            {
                anim.ResetTrigger("Damaged");  // 충돌이 끝나면 트리거 리셋
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
                // 데미지 적용
                GameManager.Instance.health -= 10;  // 적의 데미지를 정수로 적용
                lastDamageTime = currentTime;

                // Hurt 애니메이션이 재생 중이 아닐 때만 트리거
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Soldier_Hurt"))
                {
                    AudioManager.instance.PlaySfx(AudioManager.Sfx.PlayerHit);
                    anim.SetTrigger("Damaged");
                    // 트리거 리셋 추가
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

    // 트리거 리셋을 위한 코루틴 추가
    IEnumerator ResetDamageTrigger()
    {
        // Hurt 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 맞게 조정
        // Damaged 트리거 리셋
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
        isDashing = true; // 대시 중으로 설정

        // 방향에 따라 다른 애니메이션 파라미터 설정
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            // 수직 방향이 더 큰 경우 (위/아래)
            anim.SetBool(ATTACK_VERTICAL, true);
            anim.SetBool(ATTACK_HORIZONTAL, false);
        }
        else
        {
            // 수평 방향이 더 큰 경우 (왼쪽/오른쪽)
            anim.SetBool(ATTACK_HORIZONTAL, true);
            anim.SetBool(ATTACK_VERTICAL, false);
        }

        dashComponent.Init(direction);  // 대시 시작
    }

    void HandleDashEnd()
    {
        isDashing = false;
        anim.SetBool(ATTACK_VERTICAL, false);
        anim.SetBool(ATTACK_HORIZONTAL, false);

        // 대시 종료 시 면역 상태를 활성화하지 않음 (적을 맞췄을 때만 면역 활성화)
    }

    private void ActivateImmunity()
    {
        isImmune = true;
        immunityEndTime = Time.time + 1.0f;  // 면역 종료 시간 갱신
        Debug.Log("Player is immune until " + immunityEndTime);
    }
}
