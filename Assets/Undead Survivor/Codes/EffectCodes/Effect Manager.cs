using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<string, List<IEffect>> activeEffectsByTarget;

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