using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("효과 적용 대상")]
    public Player player; // ⭐ Inspector에서 Player 객체를 직접 연결

    [Header("시너지 규칙")]
    public List<SynergyRule> synergyRules = new List<SynergyRule>();

    // ⭐ 더 이상 Manager가 직접 효과 목록을 관리하지 않음
    // private Dictionary<string, List<IEffect>> activeEffectsByTarget;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ⭐ Player에게 효과 적용을 지시하는 새로운 메서드
    public void ApplyEffectToPlayer(IEffect effect)
    {
        if (player == null) return;

        // 1. 실제 효과 적용은 Player에게 위임
        player.ApplyEffect(effect);
        Debug.Log($"{player.name}에게 {effect.Name} 효과 적용을 요청합니다.");

        // 2. 시너지를 체크
        CheckAndApplySynergies();
    }

    // ⭐ Player에게 효과 제거를 지시하는 새로운 메서드
    public void RemoveEffectFromPlayer(IEffect effect)
    {
        if (player == null) return;

        player.RemoveEffect(effect);
        Debug.Log($"{player.name}에게 {effect.Name} 효과 제거를 요청합니다.");
    }

    // ⭐ 시너지 체크 로직 수정
    private void CheckAndApplySynergies()
    {
        // Player가 가진 실제 효과 목록을 가져옴
        var activeEffects = player.GetActiveEffects();
        var activeEffectIds = activeEffects.Select(e => e.EffectId).ToList();

        foreach (var rule in synergyRules)
        {
            // 이미 이 시너지 효과가 적용되어 있다면 건너뜀 (중복 방지)
            if (activeEffectIds.Contains(rule.synergyId)) continue;

            // 시너지 조건을 만족하는지 확인
            bool hasAllRequired = rule.requiredEffects.All(reqId => activeEffectIds.Contains(reqId));

            if (hasAllRequired)
            {
                Debug.Log($"시너지 발동: {rule.name}");

                // ⭐ 원본 효과를 대체해야 하는 경우
                if (rule.replaceOriginal)
                {
                    foreach (var requiredId in rule.requiredEffects)
                    {
                        var effectToRemove = activeEffects.FirstOrDefault(e => e.EffectId == requiredId);
                        if (effectToRemove != null)
                        {
                            RemoveEffectFromPlayer(effectToRemove);
                        }
                    }
                }

                // ⭐ 시너지 결과 효과들을 적용
                foreach (var effectData in rule.resultEffects)
                {
                    // EffectFactory가 있다고 가정하거나, 직접 생성
                    // 예시: IEffect newSynergyEffect = EffectFactory.Create(effectData);
                    // 우선은 간단하게 DamageEffect만 있다고 가정
                    IEffect newSynergyEffect = new DamageEffect(effectData); // 실제로는 팩토리 패턴 권장
                    ApplyEffectToPlayer(newSynergyEffect);
                }
            }
        }
    }
}