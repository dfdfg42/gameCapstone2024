using UnityEngine;
using System.Collections.Generic;

public class EffectTestUI : MonoBehaviour
{
    [Header("테스트 UI 설정")]
    public bool showDebugUI = true;
    public bool enableKeyboardTest = true;

    [Header("키보드 단축키")]
    public KeyCode testDamageKey = KeyCode.Q;
    public KeyCode testSpeedKey = KeyCode.W;
    public KeyCode testDefenseKey = KeyCode.E;
    public KeyCode testPoisonKey = KeyCode.R;
    public KeyCode testFireKey = KeyCode.T;
    public KeyCode testIceKey = KeyCode.Y;
    public KeyCode testTimeKey = KeyCode.U;
    public KeyCode clearEffectsKey = KeyCode.C;

    private Player player;
    private Vector2 scrollPosition;

    void Start()
    {
        // ▼ 수정됨: 처음에 한 번만 Player를 찾아서 성능을 최적화합니다.
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // ▼ 수정됨: 매번 FindObjectOfType을 호출하지 않도록 수정했습니다.
        if (!enableKeyboardTest || player == null) return;

        if (Input.GetKeyDown(testDamageKey))
            TestDamageEffect();
        if (Input.GetKeyDown(testSpeedKey))
            TestSpeedEffect();
        if (Input.GetKeyDown(testDefenseKey))
            TestDefenseEffect();
        if (Input.GetKeyDown(testPoisonKey))
            TestPoisonEffect();
        if (Input.GetKeyDown(testFireKey))
            TestFireEffect();
        if (Input.GetKeyDown(testIceKey))
            TestIceEffect();
        if (Input.GetKeyDown(testTimeKey))
            TestTimeEffect();
        if (Input.GetKeyDown(clearEffectsKey))
            ClearAllEffects();
    }

    void TestDamageEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Damage, value = 0.25f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("데미지 효과 테스트 적용 (+25%)");
    }

    void TestSpeedEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Speed, value = 0.3f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("속도 효과 테스트 적용 (+30%)");
    }

    void TestDefenseEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Defense, value = 3f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("방어력 효과 테스트 적용 (+3 방어력)");
    }

    void TestPoisonEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Special, specialId = "poison", value = 5f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("독 효과 테스트 적용 (5 독 데미지)");
    }

    void TestFireEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Special, specialId = "fire", value = 8f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("화염 효과 테스트 적용 (8 화염 데미지)");
    }

    void TestIceEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Special, specialId = "ice", value = 0.3f };
        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("얼음 효과 테스트 적용 (30% 빙결 확률)");
    }

    void TestTimeEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Special, specialId = "time_slow", value = 0.3f };
        var durationParam = new EffectParameter { key = "duration", type = ParameterType.Float, floatValue = 0.8f };
        effectData.parameters.Add(durationParam);

        var effect = EffectFactory.CreateEffect(effectData);
        // ▼ 수정됨: 변경된 EffectManager의 메서드를 호출합니다.
        EffectManager.Instance.ApplyEffectToPlayer(effect);
        Debug.Log("시간 느려짐 효과 테스트 적용 (0.8초간 30% 속도)");
    }

    void ClearAllEffects()
    {
        // ▼ 수정됨: 이제 Player가 직접 효과를 관리하므로 Player의 메서드를 호출합니다.
        /*if (player != null)
        {
            player.RemoveAllEffects();
            Debug.Log("모든 효과 제거");
        }*/
    }

    void OnGUI()
    {
        if (!showDebugUI || player == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 400));
        GUILayout.BeginVertical("box");

        GUILayout.Label("=== 효과 시스템 테스트 ===", GUI.skin.label);

        GUILayout.Space(10);
        GUILayout.Label("=== 키보드 단축키 ===", GUI.skin.label);
        GUILayout.Label($"[{testDamageKey}] 데미지 +25%");
        // ... (이하 생략)

        GUILayout.Space(10);
        GUILayout.Label("=== 현재 활성 효과 ===", GUI.skin.label);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        // ▼ 수정됨: Player가 효과 목록의 원본 소스이므로 Player에서 직접 가져옵니다.
        var activeEffects = player.GetActiveEffects();
        if (activeEffects.Count == 0)
        {
            GUILayout.Label("활성 효과 없음");
        }
        else
        {
            foreach (var effect in activeEffects)
            {
                string effectInfo = $"• {effect.Name}";
                if (effect.Type == EffectType.Special && effect.EffectId.StartsWith("synergy_"))
                {
                    effectInfo += " [시너지]";
                }
                GUILayout.Label(effectInfo);
            }
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}