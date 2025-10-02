using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    public KeyCode showLevelUpKey = KeyCode.I;
    public KeyCode clearEffectsKey = KeyCode.C;

    [Header("테스트용 Effect 데이터")]
    public EffectDataScriptable testDamageEffect;
    public EffectDataScriptable testSpeedEffect;
    public EffectDataScriptable testDefenseEffect;
    public EffectDataScriptable testFireEffect;
    public EffectDataScriptable testPoisonEffect;
    public EffectDataScriptable testIceEffect;

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
        if (Input.GetKeyDown(showLevelUpKey))
            ShowLevelUp();
        if (Input.GetKeyDown(clearEffectsKey))
            ClearAllEffects();
    }

    void TestDamageEffect()
    {
        if (testDamageEffect != null)
        {
            ApplyTestEffect(testDamageEffect);
            Debug.Log("데미지 효과 테스트 적용");
        }
        else
        {
            // 기본 데미지 효과
            var effectData = new EffectData { effectType = EffectType.Damage, value = 0.25f };
            var effect = EffectFactory.CreateEffect(effectData);
            EffectManager.Instance.ApplyEffect(player, effect);
            Debug.Log("데미지 효과 테스트 적용 (+25%)");
        }
    }

    void TestSpeedEffect()
    {
        if (testSpeedEffect != null)
        {
            ApplyTestEffect(testSpeedEffect);
            Debug.Log("속도 효과 테스트 적용");
        }
        else
        {
            var effectData = new EffectData { effectType = EffectType.Speed, value = 0.3f };
            var effect = EffectFactory.CreateEffect(effectData);
            EffectManager.Instance.ApplyEffect(player, effect);
            Debug.Log("속도 효과 테스트 적용 (+30%)");
        }
    }

    void TestDefenseEffect()
    {
        if (testDefenseEffect != null)
        {
            ApplyTestEffect(testDefenseEffect);
            Debug.Log("방어력 효과 테스트 적용");
        }
        else
        {
            var effectData = new EffectData { effectType = EffectType.Defense, value = 3f };
            var effect = EffectFactory.CreateEffect(effectData);
            EffectManager.Instance.ApplyEffect(player, effect);
            Debug.Log("방어력 효과 테스트 적용 (+3 방어력)");
        }
    }

    void TestPoisonEffect()
    {
        if (testPoisonEffect != null)
        {
            ApplyTestEffect(testPoisonEffect);
            Debug.Log("독 효과 테스트 적용");
        }
        else
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
    }

    void TestFireEffect()
    {
        if (testFireEffect != null)
        {
            ApplyTestEffect(testFireEffect);
            Debug.Log("화염 효과 테스트 적용");
        }
        else
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
    }

    void TestIceEffect()
    {
        if (testIceEffect != null)
        {
            ApplyTestEffect(testIceEffect);
            Debug.Log("얼음 효과 테스트 적용");
        }
        else
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
    }

    void TestTimeEffect()
    {
        var effectData = new EffectData
        {
            effectType = EffectType.Special,
            specialId = "time_slow",
            value = 0.3f
        };
        var durationParam = new EffectParameter
        {
            key = "duration",
            type = ParameterType.Float,
            floatValue = 0.8f
        };
        effectData.parameters.Add(durationParam);

        var effect = EffectFactory.CreateEffect(effectData);
        EffectManager.Instance.ApplyEffect(player, effect);
        Debug.Log("시간 느려짐 효과 테스트 적용");
    }

    void ApplyTestEffect(EffectDataScriptable effectDataScriptable)
    {
        var effectData = new EffectData
        {
            effectType = effectDataScriptable.effectType,
            value = effectDataScriptable.value,
            specialId = effectDataScriptable.specialId,
            parameters = effectDataScriptable.parameters
        };

        var effect = EffectFactory.CreateEffect(effectData);
        if (effect != null)
        {
            EffectManager.Instance.ApplyEffect(player, effect);

            // 인벤토리에 추가
            if (EffectInventory.Instance != null)
            {
                EffectInventory.Instance.AddEffect(effectDataScriptable.effectId);
            }
        }
    }

    void ShowLevelUp()
    {
        LevelUp levelUp = FindObjectOfType<LevelUp>();
        if (levelUp != null)
        {
            levelUp.Show();
            Debug.Log("레벨업 UI 표시");
        }
        else
        {
            Debug.LogWarning("LevelUp UI를 찾을 수 없습니다!");
        }
    }

    void ClearAllEffects()
    {
        if (EffectManager.Instance != null && player != null)
        {
            EffectManager.Instance.RemoveAllEffects(player);
            Debug.Log("모든 효과 제거");
        }

        if (EffectInventory.Instance != null)
        {
            EffectInventory.Instance.Reset();
            Debug.Log("효과 인벤토리 초기화");
        }

        if (EffectLevelManager.Instance != null)
        {
            EffectLevelManager.Instance.ResetLevels();
            Debug.Log("효과 레벨 초기화");
        }
    }

    void OnGUI()
    {
        if (!showDebugUI || player == null) return;

        // 메인 정보 패널
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.BeginVertical("box");

        GUILayout.Label("=== Effect 시스템 테스트 ===", GUI.skin.label);

        // 플레이어 스탯
        GUILayout.Space(10);
        GUILayout.Label("=== 플레이어 스탯 ===", GUI.skin.label);
        //GUILayout.Label($"데미지: {player.damage:F1}");
        GUILayout.Label($"속도: {player.speed:F1}");
        //GUILayout.Label($"방어력: {player.defense:F1}");

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
        GUILayout.Label($"[{showLevelUpKey}] 레벨업 UI 열기");
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

        // 보유 효과 목록
        GUILayout.Space(10);
        GUILayout.Label("=== 보유 효과 ===", GUI.skin.label);
        if (EffectInventory.Instance != null)
        {
            var ownedEffects = EffectInventory.Instance.GetAllEffects();
            if (ownedEffects.Count == 0)
            {
                GUILayout.Label("보유 효과 없음");
            }
            else
            {
                foreach (var effectId in ownedEffects)
                {
                    int level = EffectLevelManager.Instance != null
                        ? EffectLevelManager.Instance.GetEffectLevel(effectId)
                        : 0;
                    GUILayout.Label($"• {effectId} (Lv.{level})");
                }
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}