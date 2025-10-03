using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    [Header("Effect 데이터베이스")]
    public List<EffectDataScriptable> effectDatabase = new List<EffectDataScriptable>();

    [Header("시너지 설정")]
    public List<SynergyData> synergyList = new List<SynergyData>();

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        SelectRandomEffects();
        rect.localScale = Vector3.one;
        GameManager.Instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.Instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    void SelectRandomEffects()
    {
        // 0) UI 초기화
        foreach (Item item in items) item.gameObject.SetActive(false);

        // 1) 가능한 시너지 먼저 계산
        var synergyEffects = CheckAvailableSynergies(); // 아직 미보유 && 재료 모두 보유
        var synergyListThisPopup = synergyEffects.ToList(); // 복사

        // 2) 이번 팝업에서 "시너지로 인해 억제할 재료 id" 집합
        var suppressedBySynergyInThisPopup = new HashSet<string>();
        foreach (var s in synergyList)
        {
            if (s.synergyEffect == null) continue;
            // 이 시너지가 "가능"한 경우만 재료를 억제 후보에 추가
            if (synergyEffects.Any(se => se.effectId == s.synergyEffect.effectId))
            {
                foreach (var rid in s.requiredEffectIds)
                    suppressedBySynergyInThisPopup.Add(rid);
            }
        }

        // 3) 일반 후보(신규 + 업그레이드) 생성하되, 
        //    "업그레이드"는 suppressedBySynergyInThisPopup 에 속한 재료 id면 제외
        var newEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_"))
            .Where(e => !EffectManager.Instance.HasEffect(e.effectId))
            .ToList();

        var upgradableEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_"))
            .Where(e => EffectManager.Instance.HasEffect(e.effectId) && CanUpgrade(e))
            .Where(e => !suppressedBySynergyInThisPopup.Contains(e.effectId)) // ★ 재료 업그레이드 제외
            .ToList();

        // 4) 슬롯 구성: 시너지 먼저 채우고, 남은 슬롯을 신규/업그레이드로 랜덤 채움
        int slot = Mathf.Min(3, items.Length);
        var selected = new List<EffectDataScriptable>();

        // (a) 가능한 시너지부터 넣기 (여러 개면 가능한 한 많이 넣고, 남으면 자르거나 랜덤 선택해도 됨)
        foreach (var se in synergyListThisPopup)
        {
            if (selected.Count >= slot) break;
            selected.Add(se);
        }

        // (b) 남은 칸 채우기: 신규 + 업그레이드 풀을 합쳐 랜덤
        var pool = new List<EffectDataScriptable>();
        pool.AddRange(newEffects);
        pool.AddRange(upgradableEffects);

        while (selected.Count < slot && pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            selected.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // 5) UI 바인딩
        for (int i = 0; i < selected.Count; i++)
        {
            items[i].SetEffectData(selected[i]);
            items[i].gameObject.SetActive(true);
        }

        // (선택) 시너지도, 신규/업그레이드도 하나도 없으면 닫기
        if (selected.Count == 0) Hide();
    }

    List<EffectDataScriptable> GetAvailableEffects()
    {
        List<EffectDataScriptable> available = new List<EffectDataScriptable>();

        // EffectInventory 대신 EffectManager 사용
        if (EffectManager.Instance == null)
        {
            Debug.LogError("EffectManager가 없습니다!");
            return available;
        }

        // 1. 시너지 효과 체크 (최우선)
        var synergyEffects = CheckAvailableSynergies();
        available.AddRange(synergyEffects);

        // 2. 보유하지 않은 효과들
        var newEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_") &&
                       !EffectManager.Instance.HasEffect(e.effectId))
            .ToList();
        available.AddRange(newEffects);

        // 3. 이미 보유한 효과들 (레벨업 가능한 것)
        var upgradableEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_") &&
                       EffectManager.Instance.HasEffect(e.effectId) &&
                       CanUpgrade(e))
            .ToList();
        available.AddRange(upgradableEffects);

        return available;
    }

    List<EffectDataScriptable> CheckAvailableSynergies()
    {
        List<EffectDataScriptable> synergies = new List<EffectDataScriptable>();

        if (EffectManager.Instance == null)
            return synergies;

        foreach (var synergy in synergyList)
        {
            // 시너지 효과가 null이면 건너뛰기
            if (synergy.synergyEffect == null)
                continue;

            // 이미 획득한 시너지는 제외
            if (EffectManager.Instance.HasEffect(synergy.synergyEffect.effectId))
                continue;

            // 필요한 효과들을 모두 보유하고 있는지 확인
            if (EffectManager.Instance.HasAllEffects(synergy.requiredEffectIds))
            {
                synergies.Add(synergy.synergyEffect);
                Debug.Log($"시너지 가능: {synergy.synergyName}");
            }
        }

        return synergies;
    }

    bool CanUpgrade(EffectDataScriptable effect)
    {
        if (!effect.canLevelUp)
            return false;

        // EffectLevelManager 대신 EffectManager 사용
        if (EffectManager.Instance == null)
            return false;

        int currentLevel = EffectManager.Instance.GetEffectLevel(effect.effectId);
        return currentLevel < effect.maxLevel;
    }

    public void Select(int index)
    {
        if (index >= 0 && index < items.Length && items[index].gameObject.activeSelf)
        {
            items[index].OnClick();
        }
    }

    // 버튼 UI에서 호출할 메서드들
    public void OnButton0() { Select(0); }
    public void OnButton1() { Select(1); }
    public void OnButton2() { Select(2); }
}

[System.Serializable]
public class SynergyData
{
    [Header("시너지 정보")]
    public string synergyName;

    [Header("필요한 효과 ID들")]
    public List<string> requiredEffectIds = new List<string>();

    [Header("시너지 효과")]
    public EffectDataScriptable synergyEffect;

    [Header("옵션")]
    public bool replaceOriginal = true; // 원본 효과 대체 여부
}