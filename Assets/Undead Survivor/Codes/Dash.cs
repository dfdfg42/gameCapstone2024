using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    Vector2 dir;
    int damage, distance;
    const int Fdamage = 1, Fdistance = 5;

    public void Init(Vector2 direction)
    {
        this.dir = direction;

        Setting();
        Dashing();
        Effect();
    }

    protected void Setting()
    {
        damage = Fdamage;
        distance = Fdistance;

        // 타겟을 찾는 로직 등 추가 가능
        Debug.Log("Setting up dash: damage = " + damage + ", distance = " + distance);
    }

    protected void Dashing()
    {
        // 대시 거리를 계산하여 플레이어 위치를 이동
        Vector2 targetPosition = (Vector2)transform.position + dir.normalized * distance;

        // 충돌 처리 로직 - 이동 중 충돌하는 경우를 대비하여 Raycast 활용
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance);
        if (hit.collider != null)
        {
            targetPosition = hit.point;  // 충돌 지점으로 이동 위치 조정
            // 적이나 장애물 등과 충돌 시의 로직 추가 가능
            Debug.Log("Dash hit: " + hit.collider.name);
        }

        // 최종 위치로 이동
        transform.position = targetPosition;
        Debug.Log("Dashing to " + targetPosition);
    }

    protected void Effect()
    {
        // 잔상 생성, 사운드 재생, 이펙트 추가 등
        for (int i = 0; i < 5; i++)
        {
            //GameObject ghost = Instantiate(GameManager.Instance.ghostPrefab, transform.position, Quaternion.identity);
            //Destroy(ghost, 0.5f); // 잠시 후 잔상을 제거
        }

        Debug.Log("Dash effect triggered");
    }
}
