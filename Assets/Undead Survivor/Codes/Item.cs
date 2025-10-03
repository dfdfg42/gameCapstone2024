using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Text itemName;
    public Text itemDesc;
    public Text itemLevelText;
    public Image itemIcon;

    private EffectDataScriptable effectData;
    private int currentLevel;
    private int nextLevel;

    void Awake()
    {
        // UI 컴포넌트가 설정되지 않았으면 자동으로 찾기
        if (itemName == null)
            itemName = transform.Find("Name")?.GetComponent<Text>();
        if (itemDesc == null)
            itemDesc = transform.Find("Desc")?.GetComponent<Text>();
        if (itemLevelText == null)
            itemLevelText = transform.Find("Level")?.GetComponent<Text>();
        if (itemIcon == null)
            itemIcon = transform.Find("Icon")?.GetComponent<Image>();
    }

    public void SetEffectData(EffectDataScriptable data)
    {
        effectData = data;

        // 현재 레벨 확인
        currentLevel = EffectManager.Instance.GetEffectLevel(data.effectId);
        nextLevel = currentLevel + 1;

        // UI 업데이트
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (effectData == null) return;

        // 이름 표시
        if (itemName != null)
        {
            itemName.text = effectData.effectName;
        }

        // 레벨 표시
        if (itemLevelText != null)
        {
            if (currentLevel > 0)
            {
                itemLevelText.text = $"Lv.{currentLevel} → Lv.{nextLevel}";
                itemLevelText.color = Color.yellow; // 레벨업은 노란색
            }
            else
            {
                itemLevelText.text = "NEW!";
                itemLevelText.color = Color.green; // 새 효과는 초록색
            }
        }

        // 설명 표시 (다음 레벨 값으로)
        if (itemDesc != null)
        {
            float nextValue = effectData.value + (effectData.valuePerLevel * currentLevel);

            // 시너지 효과인 경우 특별 처리
            if (effectData.effectId.StartsWith("synergy_"))
            {
                itemDesc.text = effectData.description;
                if (itemLevelText != null)
                {
                    itemLevelText.text = "SYNERGY!";
                    itemLevelText.color = new Color(1f, 0.5f, 0f); // 주황색
                }
            }
            else
            {
                // 값이 포함된 설명 생성
                itemDesc.text = string.Format(effectData.description, nextValue);

                // 최대 레벨인 경우 표시
                if (effectData.canLevelUp && nextLevel >= effectData.maxLevel)
                {
                    itemDesc.text += " <color=red>(MAX)</color>";
                }
            }
        }

        // 아이콘 설정
        if (itemIcon != null && effectData.icon != null)
        {
            itemIcon.sprite = effectData.icon;
            itemIcon.enabled = true;
        }
        else if (itemIcon != null)
        {
            // 아이콘이 없으면 희귀도에 따라 기본 색상 표시
            itemIcon.enabled = true;
            itemIcon.sprite = null;
            itemIcon.color = GetRarityColor(effectData.rarity);
        }
    }

    private Color GetRarityColor(EffectRarity rarity)
    {
        switch (rarity)
        {
            case EffectRarity.Common:
                return Color.gray;
            case EffectRarity.Rare:
                return Color.blue;
            case EffectRarity.Epic:
                return new Color(0.5f, 0f, 0.5f); // 보라색
            case EffectRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // 주황색
            default:
                return Color.white;
        }
    }

    public void OnClick()
    {
        if (effectData == null) return;

        // 효과 획득 사운드
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);

        // EffectManager를 통해 효과 획득
        Player player = FindObjectOfType<Player>();
        EffectManager.Instance.AcquireEffect(effectData, player);

        // 시너지 효과인 경우 특별 메시지
        if (effectData.effectId.StartsWith("synergy_"))
        {
            Debug.Log($"시너지 발동! {effectData.effectName}");
        }

        // LevelUp UI 닫기
        LevelUp levelUp = GetComponentInParent<LevelUp>();
        if (levelUp != null)
        {
            levelUp.Hide();
        }
    }

    // 버튼 인터랙션 효과
    public void OnHover()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnHoverExit()
    {
        transform.localScale = Vector3.one;
    }

    // 효과 정보 가져오기 (디버그용)
    public EffectDataScriptable GetEffectData()
    {
        return effectData;
    }

    public bool IsMaxLevel()
    {
        if (effectData == null) return false;
        return effectData.canLevelUp && nextLevel > effectData.maxLevel;
    }
}