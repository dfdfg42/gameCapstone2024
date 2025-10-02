using UnityEngine;
using UnityEngine.UI;
using Assets.Undead_Survivor.Codes;

public class Item : MonoBehaviour
{
    public EffectDataScriptable effectData; // Effect 전용
    public int level;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;
    Image border;

    [Header("희귀도 색상")]
    public Color commonColor = new Color(0.8f, 0.8f, 0.8f);
    public Color rareColor = new Color(0.3f, 0.5f, 1f);
    public Color epicColor = new Color(0.8f, 0.3f, 1f);
    public Color legendaryColor = new Color(1f, 0.8f, 0.2f);

    void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 1)
        {
            border = images[0];
            icon = images[1];
        }

        Text[] texts = GetComponentsInChildren<Text>();
        if (texts.Length >= 3)
        {
            textLevel = texts[0];
            textName = texts[1];
            textDesc = texts[2];
        }
    }

    void OnEnable()
    {
        if (effectData != null)
        {
            UpdateDisplay();
        }
    }

    public void SetEffectData(EffectDataScriptable effect)
    {
        effectData = effect;

        if (effectData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (icon != null)
            icon.sprite = effectData.icon;

        if (textName != null)
            textName.text = effectData.effectName;

        if (textDesc != null)
        {
            // 현재 레벨의 값 계산
            int currentLevel = EffectLevelManager.Instance.GetEffectLevel(effectData.effectId);
            float currentValue = effectData.value + (effectData.valuePerLevel * currentLevel);

            // 값을 퍼센트로 표시할지 결정
            string valueText = effectData.effectType == EffectType.Damage ||
                             effectData.effectType == EffectType.Speed
                ? $"{currentValue * 100:F0}%"
                : $"{currentValue:F0}";

            textDesc.text = effectData.description.Replace("{0}", valueText);
        }

        // 희귀도에 따른 테두리 색상
        if (border != null)
        {
            switch (effectData.rarity)
            {
                case EffectRarity.Common:
                    border.color = commonColor;
                    break;
                case EffectRarity.Rare:
                    border.color = rareColor;
                    break;
                case EffectRarity.Epic:
                    border.color = epicColor;
                    break;
                case EffectRarity.Legendary:
                    border.color = legendaryColor;
                    break;
            }
        }

        // 시너지 효과인지 확인
        bool isSynergy = effectData.effectId.StartsWith("synergy_");
        if (textLevel != null)
        {
            if (isSynergy)
            {
                textLevel.text = "[시너지]";
                textLevel.color = legendaryColor;
            }
            else
            {
                // 현재 레벨 표시
                if (EffectLevelManager.Instance != null)
                {
                    int currentLevel = EffectLevelManager.Instance.GetEffectLevel(effectData.effectId);
                    textLevel.text = $"Lv.{currentLevel + 1}";

                    // 최대 레벨이면 표시
                    if (currentLevel >= effectData.maxLevel)
                    {
                        textLevel.text = "[MAX]";
                        textLevel.color = legendaryColor;
                    }
                    else
                    {
                        textLevel.color = Color.white;
                    }
                }
            }
        }
    }

    public void OnClick()
    {
        if (effectData == null) return;

        // EffectData를 실제 IEffect로 변환
        EffectData data = new EffectData
        {
            effectType = effectData.effectType,
            value = effectData.value + (effectData.valuePerLevel *
                    EffectLevelManager.Instance.GetEffectLevel(effectData.effectId)),
            specialId = effectData.specialId,
            parameters = effectData.parameters
        };

        // Effect 생성 및 적용
        IEffect effect = EffectFactory.CreateEffect(data);
        if (effect != null && GameManager.Instance.player != null)
        {
            EffectManager.Instance.ApplyEffect(GameManager.Instance.player, effect);
        }

        // 레벨 증가
        if (EffectLevelManager.Instance != null)
        {
            EffectLevelManager.Instance.IncreaseEffectLevel(effectData.effectId);
        }

        // 효과 ID를 획득 목록에 추가
        if (EffectInventory.Instance != null)
        {
            EffectInventory.Instance.AddEffect(effectData.effectId);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }

        // 레벨업 UI 닫기
        LevelUp levelUpUI = FindObjectOfType<LevelUp>();
        if (levelUpUI != null)
        {
            levelUpUI.Hide();
        }
    }
}