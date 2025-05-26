using System.Collections.Generic;
using UnityEngine;

// ========== 유물 클래스 ==========
public class Relic : IEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type { get; private set; }
    public RelicRarity Rarity { get; private set; }

    private RelicData data;
    private List<IEffect> effects;

    public Relic(RelicData relicData)
    {
        data = relicData;
        EffectId = relicData.relicId;
        Name = relicData.relicName;
        Description = relicData.description;
        Rarity = relicData.rarity;

        effects = new List<IEffect>();
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        foreach (var effectData in data.effects)
        {
            IEffect effect = EffectFactory.CreateEffect(effectData);
            if (effect != null)
                effects.Add(effect);
        }
    }

    public void Apply(IEffectTarget target)
    {
        foreach (var effect in effects)
        {
            target.ApplyEffect(effect);
        }
    }

    public void Remove(IEffectTarget target)
    {
        foreach (var effect in effects)
        {
            target.RemoveEffect(effect);
        }
    }

    public bool CanStackWith(IEffect other)
    {
        return false; // 유물은 중복 불가
    }
}