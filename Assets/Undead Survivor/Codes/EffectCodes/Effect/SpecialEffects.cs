using UnityEngine;

// ========== 액티브 효과들 (트리거 기반) ==========

public class PoisonEffect : IEffect, IActiveEffect
{
    public string EffectId => "poison";
    public string Name => "독";
    public string Description => "대쉬로 적 타격 시 독 데미지 적용";
    public EffectType Type => EffectType.Special;
    public EffectCategory Category => EffectCategory.OnHit;  // ⭐ 타격 시 발동

    private float poisonDamage;

    public PoisonEffect(EffectData data)
    {
        poisonDamage = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"독 효과 활성화: 대쉬 타격 시 {poisonDamage} 독 데미지");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("독 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "poison";
    }

    // ⭐ IActiveEffect 구현
    public bool CanTrigger(EffectTrigger trigger)
    {
        return trigger == EffectTrigger.DashHit;
    }

    public void ExecuteEffect(IEffectTarget target, object context)
    {
        if (context is Enemy enemy && enemy != null)
        {
            Debug.Log($"독 효과 발동! {enemy.name}에게 {poisonDamage} 독 데미지");

            // 실제 독 데미지 적용 로직
            if (target is Player player)
            {
                player.StartCoroutine(ApplyPoisonOverTime(enemy));
            }
        }
    }

    private System.Collections.IEnumerator ApplyPoisonOverTime(Enemy target)
    {
        int ticks = 3;
        for (int i = 0; i < ticks; i++)
        {
            if (target != null)
            {
                target.Dameged(poisonDamage / ticks);
                // 독 이펙트 표시
            }
            yield return new WaitForSeconds(1f);
        }
    }
}

public class FireEffect : IEffect, IActiveEffect
{
    public string EffectId => "fire";
    public string Name => "화염";
    public string Description => "대쉬로 적 타격 시 화염 데미지 적용";
    public EffectType Type => EffectType.Special;
    public EffectCategory Category => EffectCategory.OnHit;  // ⭐ 타격 시 발동

    private float fireDamage;

    public FireEffect(EffectData data)
    {
        fireDamage = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"화염 효과 활성화: 대쉬 타격 시 {fireDamage} 화염 데미지");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("화염 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "fire";
    }

    // ⭐ IActiveEffect 구현
    public bool CanTrigger(EffectTrigger trigger)
    {
        return trigger == EffectTrigger.DashHit;
    }

    public void ExecuteEffect(IEffectTarget target, object context)
    {
        if (context is Enemy enemy && enemy != null)
        {
            Debug.Log($"화염 효과 발동! {enemy.name}에게 {fireDamage} 화염 데미지");

            // 즉시 화염 데미지 적용
            enemy.Dameged(fireDamage);

            // 화염 이펙트 표시
            ShowFireEffect(enemy.transform.position);
        }
    }

    private void ShowFireEffect(Vector3 position)
    {
        // 1. Resources 폴더에서 파티클 프리팹을 불러옵니다.
        GameObject firePrefab = Resources.Load<GameObject>("FireEffectParticle");
        if (firePrefab == null)
        {
            Debug.LogError("FireEffectParticle 프리팹을 Resources 폴더에서 찾을 수 없습니다!");
            return;
        }

        // 2. 프리팹을 전달받은 위치(적의 위치)에 생성합니다.
        GameObject fireInstance = GameObject.Instantiate(firePrefab, position, Quaternion.identity);

        // 3. 파티클 재생이 끝나면 자동으로 파괴되도록 설정합니다.
        ParticleSystem ps = fireInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 파티클 시스템의 총 지속 시간(Duration + Start Lifetime) 이후에 게임 오브젝트를 파괴합니다.
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            GameObject.Destroy(fireInstance, totalDuration);
        }
        else
        {
            // 파티클 시스템이 없는 경우를 대비해 2초 뒤에 파괴합니다.
            GameObject.Destroy(fireInstance, 2f);
        }
    }
}

    public class IceEffect : IEffect, IActiveEffect
{
    public string EffectId => "ice";
    public string Name => "얼음";
    public string Description => "대쉬로 적 타격 시 30% 확률로 빙결";
    public EffectType Type => EffectType.Special;
    public EffectCategory Category => EffectCategory.OnHit;  // ⭐ 타격 시 발동

    private float freezeChance;

    public IceEffect(EffectData data)
    {
        freezeChance = data.value;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"얼음 효과 활성화: 대쉬 타격 시 {freezeChance * 100}% 빙결 확률");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("얼음 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return other.EffectId == "ice";
    }

    // ⭐ IActiveEffect 구현
    public bool CanTrigger(EffectTrigger trigger)
    {
        return trigger == EffectTrigger.DashHit;
    }

    public void ExecuteEffect(IEffectTarget target, object context)
    {
        if (context is Enemy enemy && enemy != null)
        {
            if (Random.Range(0f, 1f) < freezeChance)
            {
                Debug.Log($"얼음 효과 발동! {enemy.name} 빙결!");

                // 빙결 효과 적용
                if (target is Player player)
                {
                    player.StartCoroutine(FreezeEnemy(enemy));
                }
            }
            else
            {
                Debug.Log("얼음 효과 발동 실패");
            }
        }
    }

    private System.Collections.IEnumerator FreezeEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            float originalSpeed = enemy.speed;
            enemy.speed = 0f;

            // 빙결 이펙트 표시
            Debug.Log($"{enemy.name} 빙결 시작");

            yield return new WaitForSeconds(2f);

            if (enemy != null)
            {
                enemy.speed = originalSpeed;
                Debug.Log($"{enemy.name} 빙결 해제");
            }
        }
    }
}

public class TimeSlowEffect : IEffect, IActiveEffect
{
    public string EffectId => "time_slow";
    public string Name => "시간 느려짐";
    public string Description => "대쉬로 적 타격 시 시간이 느려짐";
    public EffectType Type => EffectType.Special;
    public EffectCategory Category => EffectCategory.OnHit;  // ⭐ 타격 시 발동

    private float slowFactor;
    private float duration;

    public TimeSlowEffect(EffectData data)
    {
        slowFactor = data.value;
        var paramDict = data.GetParametersDictionary();
        duration = paramDict.ContainsKey("duration") ?
            (float)paramDict["duration"] : 0.5f;
    }

    public void Apply(IEffectTarget target)
    {
        Debug.Log($"시간 느려짐 효과 활성화: 대쉬 타격 시 {duration}초간 {slowFactor}배");
    }

    public void Remove(IEffectTarget target)
    {
        Debug.Log("시간 느려짐 효과 비활성화");
    }

    public bool CanStackWith(IEffect other)
    {
        return false;
    }

    // ⭐ IActiveEffect 구현
    public bool CanTrigger(EffectTrigger trigger)
    {
        return trigger == EffectTrigger.DashHit;
    }

    public void ExecuteEffect(IEffectTarget target, object context)
    {
        Debug.Log($"시간 느려짐 효과 발동! {duration}초간 시간 {slowFactor}배");

        if (target is Player player)
        {
            player.StartCoroutine(ApplyTimeSlowEffect());
        }
    }

    private System.Collections.IEnumerator ApplyTimeSlowEffect()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = slowFactor;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        Debug.Log("시간 느려짐 효과 종료");
    }
}