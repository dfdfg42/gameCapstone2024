using System.Collections.Generic;
using System.Linq; // ? ToList(), All() 등을 위해 필요
using UnityEngine;

// ========== 통합 효과 관리자 ==========
public class EffectManager : MonoBehaviour
{
    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EffectManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EffectManager");
                    instance = go.AddComponent<EffectManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("효과 관리 데이터")]
    // 플레이어가 보유한 효과 정보 (중앙 집중식 관리)
    private Dictionary<string, EffectInfo> ownedEffects = new Dictionary<string, EffectInfo>();

    // 활성화된 효과 인스턴스 (런타임용)
    private Dictionary<IEffectTarget, List<IEffect>> activeEffects = new Dictionary<IEffectTarget, List<IEffect>>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== 효과 획득/적용 (LevelUp에서 호출) ==========
    public void AcquireEffect(EffectDataScriptable effectData, IEffectTarget target = null)
    {
        string effectId = effectData.effectId;

        // 1. 효과 정보 업데이트
        if (ownedEffects.ContainsKey(effectId))
        {
            // 이미 보유한 효과면 레벨업
            ownedEffects[effectId].level++;
            Debug.Log($"효과 레벨업: {effectId} Lv.{ownedEffects[effectId].level}");
        }
        else
        {
            // 새로운 효과 획득
            ownedEffects[effectId] = new EffectInfo
            {
                effectId = effectId,
                effectData = effectData,
                level = 1
            };
            Debug.Log($"새 효과 획득: {effectId}");
        }

        // 3) [추가] 시너지 아이템을 '선택해서' 획득한 경우, 재료 억제 처리
        if (effectId.StartsWith("synergy_"))
        {
            var levelUp = FindObjectOfType<LevelUp>();
            if (levelUp != null)
            {
                var match = levelUp.synergyList.FirstOrDefault(s =>
                    s.synergyEffect != null && s.synergyEffect.effectId == effectId);

                if (match != null && match.replaceOriginal)
                {
                    SuppressEffectsForSynergy(match.requiredEffectIds);
                }
            }
        }

        // 4) 리프레시
        if (target == null) target = FindObjectOfType<Player>();
        if (target != null) RefreshTargetEffects(target);

        
    }

    // ========== 타겟의 효과 새로고침 ==========
    private void RefreshTargetEffects(IEffectTarget target)
    {
        // 기존 효과 제거
        RemoveAllEffectsFromTarget(target);

        // 보유한 모든 효과를 타겟에 적용
        foreach (var effectInfo in ownedEffects.Values)
        {
            if (effectInfo.isActive && !effectInfo.isSuppressedBySynergy)
            {
                ApplyEffectToTarget(target, effectInfo);
            }
        }
    }

    // ========== 개별 효과 적용 ==========
    private void ApplyEffectToTarget(IEffectTarget target, EffectInfo effectInfo)
    {
        // 효과 생성 (레벨 반영)
        var effectData = CreateLeveledEffectData(effectInfo);
        IEffect effect = EffectFactory.CreateEffect(effectData);

        if (effect == null) return;

        // 타겟의 활성 효과 목록에 추가
        if (!activeEffects.ContainsKey(target))
            activeEffects[target] = new List<IEffect>();

        activeEffects[target].Add(effect);

        // 타겟에 효과 적용
        target.ApplyEffect(effect);
        Debug.Log($"효과 적용: {effectInfo.effectId} Lv.{effectInfo.level} → {target}");
    }

    // ========== 레벨이 반영된 효과 데이터 생성 ==========
    private EffectData CreateLeveledEffectData(EffectInfo effectInfo)
    {
        var baseData = effectInfo.effectData;
        float leveledValue = baseData.value + (baseData.valuePerLevel * (effectInfo.level - 1));

        return new EffectData
        {
            effectType = baseData.effectType,
            value = leveledValue,
            specialId = baseData.specialId,
            parameters = baseData.parameters
        };
    }

    // ========== 시너지 체크 및 적용 (옵션) ==========
    private void CheckAndApplySynergies()
    {
        // SynergyManager가 없어도 동작하도록 수정
        // 나중에 SynergyManager를 구현하면 여기서 연동

        // 현재는 LevelUp의 synergyList를 직접 체크
        LevelUp levelUp = FindObjectOfType<LevelUp>();
        if (levelUp != null && levelUp.synergyList != null)
        {
            foreach (var synergy in levelUp.synergyList)
            {
                if (synergy.synergyEffect == null) continue;
                if (HasEffect(synergy.synergyEffect.effectId)) continue;

                // 필요한 효과들을 모두 보유하고 있는지 확인
                if (HasAllEffects(synergy.requiredEffectIds))
                {
                    // 시너지 효과 자동 획득
                    AcquireEffect(synergy.synergyEffect);

                    // 원본 효과 억제 (옵션)
                    if (synergy.replaceOriginal)
                    {
                        SuppressEffectsForSynergy(synergy.requiredEffectIds);
                    }

                    Debug.Log($"시너지 발동: {synergy.synergyName}");
                }
            }
        }
    }

    // ========== 효과 제거 ==========
    private void RemoveAllEffectsFromTarget(IEffectTarget target)
    {
        if (!activeEffects.ContainsKey(target)) return;

        foreach (var effect in activeEffects[target])
        {
            target.RemoveEffect(effect);
        }
        activeEffects[target].Clear();
    }

    // ========== 조회 메서드들 ==========
    public bool HasEffect(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) && ownedEffects[effectId].isActive;
    }

    public bool HasAllEffects(List<string> effectIds)
    {
        return effectIds.All(id => HasEffect(id)); // System.Linq 필요
    }

    public int GetEffectLevel(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) ? ownedEffects[effectId].level : 0;
    }

    public List<string> GetOwnedEffectIds()
    {
        return ownedEffects.Keys.ToList(); // System.Linq 필요
    }

    public List<IEffect> GetTargetActiveEffects(IEffectTarget target)
    {
        return activeEffects.ContainsKey(target)
            ? new List<IEffect>(activeEffects[target])
            : new List<IEffect>();
    }

    public EffectInfo GetEffectInfo(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) ? ownedEffects[effectId] : null;
    }

    // ========== 시너지로 인한 효과 억제 ==========
    public void SuppressEffectsForSynergy(List<string> effectIds)
    {
        foreach (string id in effectIds)
        {
            if (ownedEffects.ContainsKey(id))
            {
                ownedEffects[id].isSuppressedBySynergy = true;
            }
        }

        // 모든 타겟 새로고침
        foreach (var target in activeEffects.Keys.ToList()) // ToList()로 복사본 생성
        {
            RefreshTargetEffects(target);
        }
    }

    // ========== 초기화 ==========
    public void Reset()
    {
        foreach (var kvp in activeEffects)
        {
            RemoveAllEffectsFromTarget(kvp.Key);
        }

        ownedEffects.Clear();
        activeEffects.Clear();
        Debug.Log("EffectManager 초기화 완료");
    }

    // ========== 저장/로드 ==========
    public SaveData GetSaveData()
    {
        return new SaveData
        {
            ownedEffects = ownedEffects.Values.ToList() // System.Linq 필요
        };
    }

    public void LoadSaveData(SaveData data)
    {
        ownedEffects.Clear();
        foreach (var info in data.ownedEffects)
        {
            ownedEffects[info.effectId] = info;
        }

        // 플레이어에게 효과 재적용
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            RefreshTargetEffects(player);
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public List<EffectInfo> ownedEffects;
    }
}

// ========== 효과 정보 클래스 ==========
[System.Serializable]
public class EffectInfo
{
    public string effectId;
    public EffectDataScriptable effectData;
    public int level = 1;
    public bool isActive = true;
    public bool isSuppressedBySynergy = false; // 시너지로 인해 억제됨
}