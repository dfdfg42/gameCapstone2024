using System.Collections.Generic;
using UnityEngine;

// ========== 패시브 효과들 (지속 적용) ==========

public class DamageEffect : IEffect, IPassiveEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Damage;
    public EffectCategory Category => EffectCategory.Passive;  // ⭐ 패시브

    private float damageMultiplier;

    public DamageEffect(EffectData data)
    {
        EffectId = $"damage_{data.value}";
        Name = "데미지 증가";
        Description = $"공격력 {data.value * 100}% 증가";
        damageMultiplier = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"패시브 데미지 효과 활성화: +{damageMultiplier * 100}%");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"패시브 데미지 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Damage;
    }

    // ⭐ IPassiveEffect 구현
    public float GetStatModifier(StatType statType)
    {
        return statType == StatType.Damage ? damageMultiplier : 0f;
    }
}

public class SpeedEffect : IEffect, IPassiveEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Speed;
    public EffectCategory Category => EffectCategory.Passive;  // ⭐ 패시브

    private float speedMultiplier;

    public SpeedEffect(EffectData data)
    {
        EffectId = $"speed_{data.value}";
        Name = "속도 증가";
        Description = $"이동속도 {data.value * 100}% 증가";
        speedMultiplier = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"패시브 속도 효과 활성화: +{speedMultiplier * 100}%");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"패시브 속도 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Speed;
    }

    // ⭐ IPassiveEffect 구현
    public float GetStatModifier(StatType statType)
    {
        return statType == StatType.Speed ? speedMultiplier : 0f;
    }
}

public class DefenseEffect : IEffect, IPassiveEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Defense;
    public EffectCategory Category => EffectCategory.Passive;  // ⭐ 패시브

    private float defenseValue;

    public DefenseEffect(EffectData data)
    {
        EffectId = $"defense_{data.value}";
        Name = "방어력 증가";
        Description = $"받는 데미지 {data.value} 감소";
        defenseValue = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"패시브 방어력 효과 활성화: +{defenseValue}");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"패시브 방어력 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Defense;
    }

    // ⭐ IPassiveEffect 구현
    public float GetStatModifier(StatType statType)
    {
        return statType == StatType.Defense ? defenseValue : 0f;
    }
}