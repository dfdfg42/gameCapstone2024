using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "Undead Survivor/Relic Data")]
public class RelicData : ScriptableObject
{
    [Header("기본 정보")]
    public string relicId;
    public string relicName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public RelicRarity rarity;

    [Header("효과")]
    public List<EffectData> effects = new List<EffectData>();
}