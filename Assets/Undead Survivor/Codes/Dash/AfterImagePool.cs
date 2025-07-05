// AfterImagePool.cs
using System.Collections;
using UnityEngine;

public class AfterImagePool : MonoBehaviour
{
    public GameObject afterImagePrefab; // 잔상으로 사용할 프리팹
    public float spawnInterval = 0.001f;  // 잔상 생성 간격

    private SpriteRenderer playerSpriteRenderer;

    void Awake()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartEffect()
    {
        StartCoroutine(SpawnAfterImageRoutine());
    }

    private IEnumerator SpawnAfterImageRoutine()
    {
        while (true)
        {
            GameObject instance = Instantiate(afterImagePrefab, transform.position, transform.rotation);
            SpriteRenderer instanceSr = instance.GetComponent<SpriteRenderer>();
            instanceSr.sprite = playerSpriteRenderer.sprite;
            instanceSr.flipX = playerSpriteRenderer.flipX;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void StopEffect()
    {
        StopAllCoroutines();
    }
}