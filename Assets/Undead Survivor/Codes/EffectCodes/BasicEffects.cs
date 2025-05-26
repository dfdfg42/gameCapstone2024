using System.Collections.Generic;
using UnityEngine;

// ========== 구체적인 효과 클래스들 ==========
public class DamageEffect : IEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Damage;

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
        Debug.Log($"플레이어 데미지 {damageMultiplier * 100}% 증가 적용");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"플레이어 데미지 증가 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Damage;
    }
}

public class SpeedEffect : IEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Speed;

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
        Debug.Log($"플레이어 속도 {speedMultiplier * 100}% 증가 적용");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"플레이어 속도 증가 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Speed;
    }
}

public class DefenseEffect : IEffect
{
    public string EffectId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EffectType Type => EffectType.Defense;

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
        Debug.Log($"플레이어 방어력 {defenseValue} 증가 적용");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log($"플레이어 방어력 증가 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.Type == EffectType.Defense;
    }
}