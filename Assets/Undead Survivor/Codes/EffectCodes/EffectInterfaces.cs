using System.Collections.Generic;
using UnityEngine;

// ========== 개선된 핵심 인터페이스 ==========

// 효과를 적용할 수 있는 대상 인터페이스
public interface IEffectTarget
{
    void ApplyEffect(IEffect effect);
    void RemoveEffect(IEffect effect);

    // ⭐ 추가: 이벤트 기반 효과 트리거
    void TriggerActiveEffects(EffectTrigger trigger, object context = null);
}

// 모든 효과의 기본 인터페이스
public interface IEffect
{
    string EffectId { get; }
    string Name { get; }
    string Description { get; }
    EffectType Type { get; }
    EffectCategory Category { get; }  // ⭐ 추가: 효과 카테고리
    void Apply(IEffectTarget target);
    void Remove(IEffectTarget target);
    bool CanStackWith(IEffect other);
}

// ⭐ 새로운 인터페이스: 액티브 효과
public interface IActiveEffect : IEffect
{
    bool CanTrigger(EffectTrigger trigger);
    void ExecuteEffect(IEffectTarget target, object context);
}

// ⭐ 새로운 인터페이스: 패시브 효과  
public interface IPassiveEffect : IEffect
{
    float GetStatModifier(StatType statType);
}

// ========== 새로운 열거형들 ==========

// 효과 타입 (기존)
public enum EffectType
{
    Damage,
    Speed,
    Defense,
    Special,
    Passive
}

// ⭐ 새로운: 효과 카테고리
public enum EffectCategory
{
    Passive,    // 지속적으로 스탯에 적용
    OnHit,      // 적 타격 시 발동
    OnDash,     // 대쉬 시 발동
    OnKill,     // 적 처치 시 발동
    OnDamaged,  // 피해 받을 시 발동
    Conditional // 조건부 발동
}

// ⭐ 새로운: 효과 트리거
public enum EffectTrigger
{
    DashHit,        // 대쉬로 적 타격
    EnemyKilled,    // 적 처치
    PlayerDamaged,  // 플레이어 피해
    LevelUp,        // 레벨업
    HealthLow       // 체력 부족
}

// ⭐ 새로운: 스탯 타입
public enum StatType
{
    Damage,
    Speed,
    Defense,
    Health,
    CritChance,
    CritDamage
}

// 유물 희귀도 (기존)
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