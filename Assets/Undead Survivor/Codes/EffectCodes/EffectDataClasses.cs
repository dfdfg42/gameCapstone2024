using System.Collections.Generic;
using UnityEngine;

// ========== 데이터 클래스들 ==========

// 유물 효과 데이터
[System.Serializable]
public class EffectData
{
    public EffectType effectType;
    public float value;
    public string specialId;
    public List<EffectParameter> parameters = new List<EffectParameter>();

    // Dictionary 대신 직렬화 가능한 리스트 사용
    public Dictionary<string, object> GetParametersDictionary()
    {
        var dict = new Dictionary<string, object>();
        foreach (var param in parameters)
        {
            dict[param.key] = param.GetValue();
        }
        return dict;
    }
}

[System.Serializable]
public class EffectParameter
{
    public string key;
    public ParameterType type;
    public string stringValue;
    public float floatValue;
    public int intValue;
    public bool boolValue;

    public object GetValue()
    {
        switch (type)
        {
            case ParameterType.String: return stringValue;
            case ParameterType.Float: return floatValue;
            case ParameterType.Int: return intValue;
            case ParameterType.Bool: return boolValue;
            default: return null;
        }
    }
}

// ========== 시너지 시스템 ==========
[System.Serializable]
public class SynergyRule
{
    public string synergyId;
    public string name;
    public string description;
    public List<string> requiredEffects = new List<string>(); // 필요한 효과 ID들
    public List<EffectData> resultEffects = new List<EffectData>(); // 시너지 발동시 추가 효과
    public bool replaceOriginal; // 원본 효과를 대체할지 여부
}