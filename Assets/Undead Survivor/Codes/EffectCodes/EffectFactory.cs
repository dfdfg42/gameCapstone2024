using UnityEngine;

// ========== 효과 팩토리 ==========
public static class EffectFactory
{
    public static IEffect CreateEffect(EffectData effectData)
    {
        switch (effectData.effectType)
        {
            case EffectType.Damage:
                return new DamageEffect(effectData);
            case EffectType.Speed:
                return new SpeedEffect(effectData);
            case EffectType.Defense:
                return new DefenseEffect(effectData);
            case EffectType.Special:
                return CreateSpecialEffect(effectData);
            default:
                Debug.LogWarning($"알 수 없는 효과 타입: {effectData.effectType}");
                return null;
        }
    }

    private static IEffect CreateSpecialEffect(EffectData effectData)
    {
        switch (effectData.specialId)
        {
            case "time_slow":
                return new TimeSlowEffect(effectData);
            case "poison":
                return new PoisonEffect(effectData);
            case "fire":
                return new FireEffect(effectData);
            case "ice":
                return new IceEffect(effectData);
            default:
                Debug.LogWarning($"알 수 없는 특수 효과: {effectData.specialId}");
                return null;
        }
    }
}