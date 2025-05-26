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
    public KeyCode testRandomRelicKey = KeyCode.I;
    public KeyCode clearEffectsKey = KeyCode.C;

    private Player player;
    private Vector2 scrollPosition;

    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        player = FindObjectOfType<Player>();
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
        if (Input.GetKeyDown(testRandomRelicKey))
            TestRandomRelic();
        if (Input.GetKeyDown(clearEffectsKey))
            ClearAllEffects();
    }

    void TestDamageEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Damage, value = 0.25f };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("데미지 효과 테스트 적용 (+25%)");
    }

    void TestSpeedEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Speed, value = 0.3f };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("속도 효과 테스트 적용 (+30%)");
    }

    void TestDefenseEffect()
    {
        var effectData = new EffectData { effectType = EffectType.Defense, value = 3f };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("방어력 효과 테스트 적용 (+3 방어력)");
    }

    void TestPoisonEffect()
    {
        var effectData = new EffectData
        {
            effectType = EffectType.Special,
            specialId = "poison",
            value = 5f
        };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("독 효과 테스트 적용 (5 독 데미지)");
    }

    void TestFireEffect()
    {
        var effectData = new EffectData
        {
            effectType = EffectType.Special,
            specialId = "fire",
            value = 8f
        };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("화염 효과 테스트 적용 (8 화염 데미지)");
    }

    void TestIceEffect()
    {
        var effectData = new EffectData
        {
            effectType = EffectType.Special,
            specialId = "ice",
            value = 0.3f
        };
        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("얼음 효과 테스트 적용 (30% 빙결 확률)");
    }

    void TestTimeEffect()
    {
        var effectData = new EffectData
        {
            effectType = EffectType.Special,
            specialId = "time_slow",
            value = 0.3f
        };
        var durationParam = new EffectParameter();
        durationParam.key = "duration";
        durationParam.type = ParameterType.Float;
        durationParam.floatValue = 0.8f;
        effectData.parameters.Add(durationParam);

        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("시간 느려짐 효과 테스트 적용 (0.8초간 30% 속도)");
    }

    void TestRandomRelic()
    {
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.AcquireRandomRelic();
            Debug.Log("랜덤 유물 획득!");
        }
    }

    void ClearAllEffects()
    {
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.RemoveAllEffects(player);
            Debug.Log("모든 효과 제거");
        }
    }

    void OnGUI()
    {
        if (!showDebugUI || player == null) return;

        // 메인 정보 패널
        GUILayout.BeginArea(new Rect(10, 10, 350, 400));
        GUILayout.BeginVertical("box");

        GUILayout.Label("=== 효과 시스템 테스트 ===", GUI.skin.label);

        // 키 가이드
        GUILayout.Space(10);
        GUILayout.Label("=== 키보드 단축키 ===", GUI.skin.label);
        GUILayout.Label($"[{testDamageKey}] 데미지 +25%");
        GUILayout.Label($"[{testSpeedKey}] 속도 +30%");
        GUILayout.Label($"[{testDefenseKey}] 방어력 +3");
        GUILayout.Label($"[{testPoisonKey}] 독 효과");
        GUILayout.Label($"[{testFireKey}] 화염 효과");
        GUILayout.Label($"[{testIceKey}] 얼음 효과");
        GUILayout.Label($"[{testTimeKey}] 시간 느려짐");
        GUILayout.Label($"[{testRandomRelicKey}] 랜덤 유물");
        GUILayout.Label($"[{clearEffectsKey}] 효과 초기화");

        // 현재 활성 효과
        GUILayout.Space(10);
        GUILayout.Label("=== 현재 활성 효과 ===", GUI.skin.label);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        var activeEffects = EffectManager.Instance.GetActiveEffects(player);
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

        // 소유 유물 목록
        GUILayout.Space(10);
        GUILayout.Label("=== 소유 유물 ===", GUI.skin.label);
        if (RelicManager.Instance != null)
        {
            var ownedRelics = RelicManager.Instance.GetOwnedRelics();
            if (ownedRelics.Count == 0)
            {
                GUILayout.Label("소유 유물 없음");
            }
            else
            {
                foreach (var relic in ownedRelics)
                {
                    string rarityColor = GetRarityColor(relic.Rarity);
                    GUILayout.Label($"• {relic.Name} ({rarityColor})");
                }
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    string GetRarityColor(RelicRarity rarity)
    {
        switch (rarity)
        {
            case RelicRarity.Common: return "일반";
            case RelicRarity.Rare: return "희귀";
            case RelicRarity.Epic: return "영웅";
            case RelicRarity.Legendary: return "전설";
            default: return "알 수 없음";
        }
    }
}