using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    public Text itemName;
    public Text itemDesc;
    public Text itemLevelText;
    public Image itemIcon;

    private EffectDataScriptable effectData;
    private int currentLevel;
    private int nextLevel;

    void Awake()
    {
        // UI ������Ʈ�� �������� �ʾ����� �ڵ����� ã��
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

        // ���� ���� Ȯ��
        currentLevel = EffectManager.Instance.GetEffectLevel(data.effectId);
        nextLevel = currentLevel + 1;

        // UI ������Ʈ
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (effectData == null) return;

        // �̸� ǥ��
        if (itemName != null)
        {
            itemName.text = effectData.effectName;
        }

        // ���� ǥ��
        if (itemLevelText != null)
        {
            if (currentLevel > 0)
            {
                itemLevelText.text = $"Lv.{currentLevel} �� Lv.{nextLevel}";
                itemLevelText.color = Color.yellow; // �������� �����
            }
            else
            {
                itemLevelText.text = "NEW!";
                itemLevelText.color = Color.green; // �� ȿ���� �ʷϻ�
            }
        }

        // ���� ǥ�� (���� ���� ������)
        if (itemDesc != null)
        {
            float nextValue = effectData.value + (effectData.valuePerLevel * currentLevel);

            // �ó��� ȿ���� ��� Ư�� ó��
            if (effectData.effectId.StartsWith("synergy_"))
            {
                itemDesc.text = effectData.description;
                if (itemLevelText != null)
                {
                    itemLevelText.text = "SYNERGY!";
                    itemLevelText.color = new Color(1f, 0.5f, 0f); // ��Ȳ��
                }
            }
            else
            {
                // ���� ���Ե� ���� ����
                itemDesc.text = string.Format(effectData.description, nextValue);

                // �ִ� ������ ��� ǥ��
                if (effectData.canLevelUp && nextLevel >= effectData.maxLevel)
                {
                    itemDesc.text += " <color=red>(MAX)</color>";
                }
            }
        }

        // ������ ����
        if (itemIcon != null && effectData.icon != null)
        {
            itemIcon.sprite = effectData.icon;
            itemIcon.enabled = true;
        }
        else if (itemIcon != null)
        {
            // �������� ������ ��͵��� ���� �⺻ ���� ǥ��
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
                return new Color(0.5f, 0f, 0.5f); // �����
            case EffectRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // ��Ȳ��
            default:
                return Color.white;
        }
    }

    public void OnClick()
    {
        if (effectData == null) return;

        // ȿ�� ȹ�� ����
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);

        // EffectManager�� ���� ȿ�� ȹ��
        Player player = FindObjectOfType<Player>();
        EffectManager.Instance.AcquireEffect(effectData, player);

        // �ó��� ȿ���� ��� Ư�� �޽���
        if (effectData.effectId.StartsWith("synergy_"))
        {
            Debug.Log($"�ó��� �ߵ�! {effectData.effectName}");
        }

        // LevelUp UI �ݱ�
        LevelUp levelUp = GetComponentInParent<LevelUp>();
        if (levelUp != null)
        {
            levelUp.Hide();
        }
    }

    // ��ư ���ͷ��� ȿ��
    public void OnHover()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnHoverExit()
    {
        transform.localScale = Vector3.one;
    }

    // ȿ�� ���� �������� (����׿�)
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