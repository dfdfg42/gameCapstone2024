using System.Collections.Generic;
using UnityEngine;

// ========== ������ Ŭ������ ==========

// ���� ȿ�� ������
[System.Serializable]
public class EffectData
{
    public EffectType effectType;
    public float value;
    public string specialId;
    public List<EffectParameter> parameters = new List<EffectParameter>();

    // Dictionary ��� ����ȭ ������ ����Ʈ ���
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

// ========== �ó��� �ý��� ==========
[System.Serializable]
public class SynergyRule
{
    public string synergyId;
    public string name;
    public string description;
    public List<string> requiredEffects = new List<string>(); // �ʿ��� ȿ�� ID��
    public List<EffectData> resultEffects = new List<EffectData>(); // �ó��� �ߵ��� �߰� ȿ��
    public bool replaceOriginal; // ���� ȿ���� ��ü���� ����
}