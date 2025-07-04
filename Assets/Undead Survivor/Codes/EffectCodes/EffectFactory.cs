using UnityEngine;

// ========== ȿ�� ���丮 ==========
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
                Debug.LogWarning($"�� �� ���� ȿ�� Ÿ��: {effectData.effectType}");
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
                Debug.LogWarning($"�� �� ���� Ư�� ȿ��: {effectData.specialId}");
                return null;
        }
    }
}