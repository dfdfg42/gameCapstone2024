using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : MonoBehaviour
{
    public float damage = 3f; //일단 3으로
    public float speed = 3f;  // 총알 속도(일단 3으로)

    Vector2 direction;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, Vector2 direction)
    {
        this.damage = damage;
        this.direction = direction;
    }

    void FixedUpdate()
    {
        // 총알 이동
        rigid.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌했을 때
        if (collision.CompareTag("Player"))
        {
            // 플레이어 데미지 처리
            GameManager.Instance.health -= damage;
            if (GameManager.Instance.health <= 0)
            {
                collision.gameObject.GetComponent<Player>().onDeath();
            }
            // 총알 비활성화
            gameObject.SetActive(false);
        }
        
    }
}