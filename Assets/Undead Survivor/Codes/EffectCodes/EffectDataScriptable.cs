
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Undead Survivor/Effect Data")]
public class EffectDataScriptable : ScriptableObject
{
    [Header("기본 정보")]
    public string effectId;
    public string effectName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public EffectRarity rarity;

    [Header("효과 설정")]
    public EffectType effectType;
    public float value;
    public string specialId; // Special 타입일 때 사용

    [Header("추가 파라미터")]
    public List<EffectParameter> parameters = new List<EffectParameter>();

    [Header("레벨업 설정")]
    public bool canLevelUp = true;
    public int maxLevel = 5;
    public float valuePerLevel = 0.05f; // 레벨당 증가량
}

public enum EffectRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}