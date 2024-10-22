using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;

    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    public GameObject bulletPrefab; // Bullet 프리팹을 연결
    public GameObject ghostPrefab; // 잔상 고스트 프리팹

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    private bool canDash = true;
    private Vector2 lastMoveDir; // 마지막으로 움직인 방향


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.Instance.playerId];
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            OnDash();
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive || isChronoActive)
            return;

        if (inputVec != Vector2.zero)
        {
            lastMoveDir = inputVec; // 마지막으로 움직인 방향을 저장
        }

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

    public void OnDash()
    {
        if (canDash)
        {
        }
    }




}
