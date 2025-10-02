using System.Collections.Generic;
using UnityEngine;

// ========== 시너지 효과 ==========
public class SynergyEffect : IEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type { get; private set; }
    public EffectCategory Category { get; private set; }

    private List<IEffect> combinedEffects;

    public SynergyEffect(SynergyRule rule)
    {
        EffectId = rule.synergyId;
        Name = rule.name;
        Description = rule.description;
        Type = EffectType.Special;

        combinedEffects = new List<IEffect>();
        foreach (var effectData in rule.resultEffects)
        {
            var effect = EffectFactory.CreateEffect(effectData);
            if (effect != null)
                combinedEffects.Add(effect);
        }
    }

    public void Apply(IEffectTarget target)
    {
        foreach (var effect in combinedEffects)
        {
            target.ApplyEffect(effect);
        }
    }

    public void Remove(IEffectTarget target)
    {
        foreach (var effect in combinedEffects)
        {
            target.RemoveEffect(effect);
        }
    }

    public bool CanStackWith(IEffect other)
    {
        return false; // 시너지 효과는 중복 불가
    }
}