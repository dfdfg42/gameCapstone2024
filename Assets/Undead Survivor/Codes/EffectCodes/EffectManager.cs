using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ========== 효과 매니저 (MonoBehaviour) ==========
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("시너지 규칙")]
    public List<SynergyRule> synergyRules = new List<SynergyRule>();

    [Header("유물 데이터")]
    public List<RelicData> relicDatabase = new List<RelicData>();

    private Dictionary<string, List<IEffect>> activeEffectsByTarget;
    private Dictionary<string, SynergyRule> synergyRulesDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        activeEffectsByTarget = new Dictionary<string, List<IEffect>>();
        synergyRulesDict = new Dictionary<string, SynergyRule>();

        // 시너지 규칙 딕셔너리 초기화
        foreach (var rule in synergyRules)
        {
            synergyRulesDict[rule.synergyId] = rule;
        }
    }

    // 효과 적용
    public void ApplyEffect(IEffectTarget target, IEffect effect)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
        {
            activeEffectsByTarget[targetId] = new List<IEffect>();
        }

        var targetEffects = activeEffectsByTarget[targetId];

        // 스택 가능한 효과인지 확인
        var existingEffect = targetEffects.FirstOrDefault(e => e.EffectId == effect.EffectId);
        if (existingEffect != null)
        {
            if (!effect.CanStackWith(existingEffect))
            {
                Debug.Log($"효과 {effect.Name}는 중복 적용할 수 없습니다.");
                return;
            }
        }

        // 효과 적용
        effect.Apply(target);
        targetEffects.Add(effect);

        Debug.Log($"{GetTargetName(target)}에게 {effect.Name} 효과 적용");

        // 시너지 체크
        CheckAndApplySynergies(target);
    }

    // 효과 제거
    public void RemoveEffect(IEffectTarget target, string effectId)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
            return;

        var targetEffects = activeEffectsByTarget[targetId];
        var effectToRemove = targetEffects.FirstOrDefault(e => e.EffectId == effectId);

        if (effectToRemove != null)
        {
            effectToRemove.Remove(target);
            targetEffects.Remove(effectToRemove);

            Debug.Log($"{GetTargetName(target)}에서 {effectToRemove.Name} 효과 제거");

            // 시너지 재체크
            CheckAndApplySynergies(target);
        }
    }

    // 모든 효과 제거
    public void RemoveAllEffects(IEffectTarget target)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
            return;

        var targetEffects = activeEffectsByTarget[targetId];

        foreach (var effect in targetEffects.ToList())
        {
            effect.Remove(target);
        }

        targetEffects.Clear();
        Debug.Log($"{GetTargetName(target)}의 모든 효과 제거");
    }

    // 시너지 체크 및 적용
    private void CheckAndApplySynergies(IEffectTarget target)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
            return;

        var targetEffects = activeEffectsByTarget[targetId];
        var activeEffectIds = targetEffects.Select(e => e.EffectId).ToList();

        // 기존 시너지 효과 제거
        var synergiesToRemove = targetEffects.Where(e => e.Type == EffectType.Special &&
                                                   e.EffectId.StartsWith("synergy_")).ToList();
        foreach (var synergy in synergiesToRemove)
        {
            synergy.Remove(target);
            targetEffects.Remove(synergy);
        }

        // 새로운 시너지 체크
        foreach (var rule in synergyRules)
        {
            if (HasRequiredEffects(activeEffectIds, rule.requiredEffects))
            {
                var synergyEffect = new SynergyEffect(rule);
                synergyEffect.Apply(target);
                targetEffects.Add(synergyEffect);

                Debug.Log($"시너지 발동: {rule.name}");

                // 원본 효과 대체가 필요한 경우
                if (rule.replaceOriginal)
                {
                    foreach (var requiredId in rule.requiredEffects)
                    {
                        var originalEffect = targetEffects.FirstOrDefault(e => e.EffectId == requiredId);
                        if (originalEffect != null)
                        {
                            originalEffect.Remove(target);
                            targetEffects.Remove(originalEffect);
                        }
                    }
                }
            }
        }
    }

    // 필요한 효과들이 모두 있는지 확인
    private bool HasRequiredEffects(List<string> activeEffects, List<string> requiredEffects)
    {
        return requiredEffects.All(required => activeEffects.Contains(required));
    }

    // 타겟의 고유 ID 반환
    private string GetTargetId(IEffectTarget target)
    {
        if (target is MonoBehaviour mono)
            return mono.GetInstanceID().ToString();
        return target.GetHashCode().ToString();
    }

    // 타겟의 이름 반환
    private string GetTargetName(IEffectTarget target)
    {
        if (target is MonoBehaviour mono)
            return mono.name;
        return target.GetType().Name;
    }

    // 현재 활성화된 효과 목록 반환
    public List<IEffect> GetActiveEffects(IEffectTarget target)
    {
        string targetId = GetTargetId(target);

        if (activeEffectsByTarget.ContainsKey(targetId))
            return new List<IEffect>(activeEffectsByTarget[targetId]);

        return new List<IEffect>();
    }
}