using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Dash dashComponent;

    private bool isDashing = false;  // 대시 상태 플래그

	// Dash 애니메이션을 위한 파라미터 추가
	private readonly string ATTACK_VERTICAL = "AttackVertical";
	private readonly string ATTACK_HORIZONTAL = "AttackHorizontal";

	void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
        dashComponent = GetComponent<Dash>();

        // Dash 컴포넌트에서 대시가 끝났을 때 호출할 수 있도록 이벤트 설정
        if (dashComponent != null)
        {
            dashComponent.OnDashEnd += HandleDashEnd;
        }
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.Instance.playerId];
    }

    private void Update()
    {

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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.Instance.isLive)
            return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        //기본 몬스터의 근접 공격
        if (enemy != null)
        {
            GameManager.Instance.health -= Time.deltaTime * 10;

			if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Soldier_Hurt"))  // 현재 Hurt 애니메이션이 재생 중이 아닐 때만
			{
				anim.SetTrigger("Damaged");
			}
		}
        if (GameManager.Instance.health < 0)
        {
            this.onDeath();
        }
    }

    public void onDeath()
    {
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
       // anim.SetBool("Attack", true); // attack 애니메이션 시작

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
        isDashing = false;  // 대시가 끝나면 대시 중 아님으로 설정
							//anim.SetBool("Attack", false); // attack 애니메이션 종료

		// 모든 공격 애니메이션 파라미터 리셋
		anim.SetBool(ATTACK_VERTICAL, false);
		anim.SetBool(ATTACK_HORIZONTAL, false);
	}
}