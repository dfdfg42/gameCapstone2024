using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : MonoBehaviour
{
    public float damage = 3f; //�ϴ� 3����
    public float speed = 3f;  // �Ѿ� �ӵ�(�ϴ� 3����)

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
        // �Ѿ� �̵�
        rigid.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // �÷��̾�� �浹���� ��
        if (collision.CompareTag("Player"))
        {
            // �÷��̾� ������ ó��
            GameManager.Instance.health -= damage;
            // �Ѿ� ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
        
    }
}
