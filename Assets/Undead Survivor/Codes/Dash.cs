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

        // Ÿ���� ã�� ���� �� �߰� ����
        Debug.Log("Setting up dash: damage = " + damage + ", distance = " + distance);
    }

    protected void Dashing()
    {
        // ��� �Ÿ��� ����Ͽ� �÷��̾� ��ġ�� �̵�
        Vector2 targetPosition = (Vector2)transform.position + dir.normalized * distance;

        // �浹 ó�� ���� - �̵� �� �浹�ϴ� ��츦 ����Ͽ� Raycast Ȱ��
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance);
        if (hit.collider != null)
        {
            targetPosition = hit.point;  // �浹 �������� �̵� ��ġ ����
            // ���̳� ��ֹ� ��� �浹 ���� ���� �߰� ����
            Debug.Log("Dash hit: " + hit.collider.name);
        }

        // ���� ��ġ�� �̵�
        transform.position = targetPosition;
        Debug.Log("Dashing to " + targetPosition);
    }

    protected void Effect()
    {
        // �ܻ� ����, ���� ���, ����Ʈ �߰� ��
        for (int i = 0; i < 5; i++)
        {
            //GameObject ghost = Instantiate(GameManager.Instance.ghostPrefab, transform.position, Quaternion.identity);
            //Destroy(ghost, 0.5f); // ��� �� �ܻ��� ����
        }

        Debug.Log("Dash effect triggered");
    }
}
