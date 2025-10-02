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
        // 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 사용 가능한 효과 목록 생성
        List<EffectDataScriptable> availableEffects = GetAvailableEffects();

        if (availableEffects.Count == 0)
        {
            Debug.LogWarning("사용 가능한 효과가 없습니다!");
            Hide();
            return;
        }

        // 랜덤으로 3개 선택
        int selectCount = Mathf.Min(3, Mathf.Min(availableEffects.Count, items.Length));
        List<EffectDataScriptable> selectedEffects = new List<EffectDataScriptable>();

        for (int i = 0; i < selectCount; i++)
        {
            int randomIndex = Random.Range(0, availableEffects.Count);
            selectedEffects.Add(availableEffects[randomIndex]);
            availableEffects.RemoveAt(randomIndex);
        }

        // 선택된 효과 표시
        for (int i = 0; i < selectedEffects.Count; i++)
        {
            items[i].SetEffectData(selectedEffects[i]);
            items[i].gameObject.SetActive(true);
        }
    }

    List<EffectDataScriptable> GetAvailableEffects()
    {
        List<EffectDataScriptable> available = new List<EffectDataScriptable>();

        if (EffectInventory.Instance == null)
        {
            Debug.LogError("EffectInventory가 없습니다!");
            return available;
        }

        // 1. 시너지 효과 체크 (최우선)
        var synergyEffects = CheckAvailableSynergies();
        available.AddRange(synergyEffects);

        // 2. 보유하지 않은 효과들
        var newEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_") &&
                       !EffectInventory.Instance.HasEffect(e.effectId))
            .ToList();
        available.AddRange(newEffects);

        // 3. 이미 보유한 효과들 (레벨업 가능한 것)
        var upgradableEffects = effectDatabase
            .Where(e => !e.effectId.StartsWith("synergy_") &&
                       EffectInventory.Instance.HasEffect(e.effectId) &&
                       CanUpgrade(e))
            .ToList();
        available.AddRange(upgradableEffects);

        return available;
    }

    List<EffectDataScriptable> CheckAvailableSynergies()
    {
        List<EffectDataScriptable> synergies = new List<EffectDataScriptable>();

        if (EffectInventory.Instance == null)
            return synergies;

        foreach (var synergy in synergyList)
        {
            // 시너지 효과가 null이면 건너뛰기
            if (synergy.synergyEffect == null)
                continue;

            // 이미 획득한 시너지는 제외
            if (EffectInventory.Instance.HasEffect(synergy.synergyEffect.effectId))
                continue;

            // 필요한 효과들을 모두 보유하고 있는지 확인
            if (EffectInventory.Instance.HasAllEffects(synergy.requiredEffectIds))
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

        if (EffectLevelManager.Instance == null)
            return false;

        int currentLevel = EffectLevelManager.Instance.GetEffectLevel(effect.effectId);
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