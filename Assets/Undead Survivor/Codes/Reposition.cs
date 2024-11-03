using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D coll;
    private const float CHECK_FREQUENCY = 0.1f;  // 위치 체크 주기
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

        //현재 타일이 어느 사분면에 있는지 확인
        bool isRight = myPos.x > playerPos.x;
        bool isUp = myPos.y > playerPos.y;

        //플레이어 기준으로 새로운 타일 위치 계산
        float newX = playerPos.x + (isRight ? 10 : -10);
        float newY = playerPos.y + (isUp ? 10 : -10);

        // 현재 위치 - 목표 위치 > 1 이면 이동
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