using System.Collections.Generic;
using UnityEngine;

// ========== 핵심 인터페이스 ==========

// 효과를 적용할 수 있는 대상 인터페이스
public interface IEffectTarget
{
    void ApplyEffect(IEffect effect);
    void RemoveEffect(IEffect effect);
}

// 모든 효과의 기본 인터페이스
public interface IEffect
{
    string EffectId { get; }
    string Name { get; }
    string Description { get; }
    EffectType Type { get; }
    void Apply(IEffectTarget target);
    void Remove(IEffectTarget target);
    bool CanStackWith(IEffect other);
}

// ========== 열거형들 ==========

// 효과 타입 열거형
public enum EffectType
{
    Damage,
    Speed,
    Defense,
    Special,
    Passive
}

// 유물 희귀도 열거형
public enum RelicRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum ParameterType
{
    String,
    Float,
    Int,
    Bool
}