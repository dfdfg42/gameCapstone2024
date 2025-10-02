
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Undead Survivor/Effect Data")]
public class EffectDataScriptable : ScriptableObject
{
    [Header("�⺻ ����")]
    public string effectId;
    public string effectName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public EffectRarity rarity;

    [Header("ȿ�� ����")]
    public EffectType effectType;
    public float value;
    public string specialId; // Special Ÿ���� �� ���

    [Header("�߰� �Ķ����")]
    public List<EffectParameter> parameters = new List<EffectParameter>();

    [Header("������ ����")]
    public bool canLevelUp = true;
    public int maxLevel = 5;
    public float valuePerLevel = 0.05f; // ������ ������
}

public enum EffectRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}