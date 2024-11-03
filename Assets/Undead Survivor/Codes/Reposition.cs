using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D coll;
    private const float CHECK_FREQUENCY = 0.1f;  // ��ġ üũ �ֱ�
    private float checkTimer = 0;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (transform.CompareTag("Ground"))
        {
            //update timer
            checkTimer += Time.fixedDeltaTime;

            if (checkTimer >= CHECK_FREQUENCY)
            {
                checkTimer = 0;
                RepositionTile();
            }
        }
    }

    void RepositionTile()
    {
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 myPos = transform.position;

        //���� Ÿ���� ��� ��и鿡 �ִ��� Ȯ��
        bool isRight = myPos.x > playerPos.x;
        bool isUp = myPos.y > playerPos.y;

        //�÷��̾� �������� ���ο� Ÿ�� ��ġ ���
        float newX = playerPos.x + (isRight ? 10 : -10);
        float newY = playerPos.y + (isUp ? 10 : -10);

        // ���� ��ġ - ��ǥ ��ġ > 1 �̸� �̵�
        if (Vector2.Distance(myPos, new Vector2(newX, newY)) > 1f)
        {
            transform.position = new Vector3(newX, newY, 0);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;

        if (transform.CompareTag("Enemy") && coll.enabled)
        {
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            Vector3 myPos = transform.position;
            Vector3 dist = playerPos - myPos;
            Vector3 ran = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
            transform.Translate(ran + dist * 2);
        }
    }
}