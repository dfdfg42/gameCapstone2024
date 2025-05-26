using UnityEngine;

// ========== 특수 효과들 ==========
public class TimeSlowEffect : IEffect
{
    public string EffectId => "time_slow";
    public string Name => "시간 느려짐";
    public string Description => "공격 성공 시 시간이 느려집니다";
    public EffectType Type => EffectType.Special;

    private float slowDuration;
    private float slowFactor;

    public TimeSlowEffect(EffectData data)
    {
        slowDuration = data.parameters?.Count > 0 &&
                      data.GetParametersDictionary().ContainsKey("duration") ?
                      (float)data.GetParametersDictionary()["duration"] : 0.5f;
        slowFactor = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"시간 느려짐 효과 적용: {slowFactor}배, {slowDuration}초");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("시간 느려짐 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return false;
    }
}

public class PoisonEffect : IEffect
{
    public string EffectId => "poison";
    public string Name => "독";
    public string Description => "공격에 독 데미지가 추가됩니다";
    public EffectType Type => EffectType.Special;

    private float poisonDamage;

    public PoisonEffect(EffectData data)
    {
        poisonDamage = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"독 효과 적용: {poisonDamage} 독 데미지");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("독 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "poison";
    }
}

public class FireEffect : IEffect
{
    public string EffectId => "fire";
    public string Name => "화염";
    public string Description => "공격에 화염 데미지가 추가됩니다";
    public EffectType Type => EffectType.Special;

    private float fireDamage;

    public FireEffect(EffectData data)
    {
        fireDamage = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"화염 효과 적용: {fireDamage} 화염 데미지");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("화염 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "fire";
    }
}

public class IceEffect : IEffect
{
    public string EffectId => "ice";
    public string Name => "얼음";
    public string Description => "공격에 빙결 효과가 추가됩니다";
    public EffectType Type => EffectType.Special;

    private float freezeChance;

    public IceEffect(EffectData data)
    {
        freezeChance = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"얼음 효과 적용: {freezeChance * 100}% 빙결 확률");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("얼음 효과 제거");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "ice";
    }
}