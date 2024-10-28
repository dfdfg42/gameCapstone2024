using UnityEngine.InputSystem;
using UnityEngine;

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
        if (Input.GetButtonDown("Jump") && !isDashing) // 대시 중이 아닐 때만 대시 가능
        {
            OnDash();
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

        GameManager.Instance.health -= Time.deltaTime * 10;

        if (GameManager.Instance.health < 0)
        {
            for (int index = 2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.Instance.GameOver();
        }
    }

    void OnDash()
    {
        Vector2 direction = inputVec.normalized;
        isDashing = true; // 대시 중으로 설정
        dashComponent.Init(direction);  // 대시 시작
    }

    void HandleDashEnd()
    {
        isDashing = false;  // 대시가 끝나면 대시 중 아님으로 설정
    }
}
