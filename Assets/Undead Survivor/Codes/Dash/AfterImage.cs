using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer sr;
    private float alpha;
    public float alphaSet = 0.8f; // 잔상이 처음 나타날 때의 투명도
    public float alphaMultiplier = 0.85f; // 매 프레임마다 알파 값이 얼마나 줄어들지

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        alpha = alphaSet;
        sr.color = new Color(1, 1, 1, alpha);
    }

    void Update()
    {
        alpha *= alphaMultiplier;
        sr.color = new Color(1, 1, 1, alpha);

        if (alpha <= 0.05f)
        {
            Destroy(gameObject); // 알파 값이 거의 0에 가까워지면 오브젝트 파괴
        }
    }
}