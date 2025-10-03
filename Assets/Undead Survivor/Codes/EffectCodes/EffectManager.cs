using System.Collections.Generic;
using System.Linq; // ? ToList(), All() ���� ���� �ʿ�
using UnityEngine;

// ========== ���� ȿ�� ������ ==========
public class EffectManager : MonoBehaviour
{
    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EffectManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EffectManager");
                    instance = go.AddComponent<EffectManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("ȿ�� ���� ������")]
    // �÷��̾ ������ ȿ�� ���� (�߾� ���߽� ����)
    private Dictionary<string, EffectInfo> ownedEffects = new Dictionary<string, EffectInfo>();

    // Ȱ��ȭ�� ȿ�� �ν��Ͻ� (��Ÿ�ӿ�)
    private Dictionary<IEffectTarget, List<IEffect>> activeEffects = new Dictionary<IEffectTarget, List<IEffect>>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== ȿ�� ȹ��/���� (LevelUp���� ȣ��) ==========
    public void AcquireEffect(EffectDataScriptable effectData, IEffectTarget target = null)
    {
        string effectId = effectData.effectId;

        // 1. ȿ�� ���� ������Ʈ
        if (ownedEffects.ContainsKey(effectId))
        {
            // �̹� ������ ȿ���� ������
            ownedEffects[effectId].level++;
            Debug.Log($"ȿ�� ������: {effectId} Lv.{ownedEffects[effectId].level}");
        }
        else
        {
            // ���ο� ȿ�� ȹ��
            ownedEffects[effectId] = new EffectInfo
            {
                effectId = effectId,
                effectData = effectData,
                level = 1
            };
            Debug.Log($"�� ȿ�� ȹ��: {effectId}");
        }

        // 3) [�߰�] �ó��� �������� '�����ؼ�' ȹ���� ���, ��� ���� ó��
        if (effectId.StartsWith("synergy_"))
        {
            var levelUp = FindObjectOfType<LevelUp>();
            if (levelUp != null)
            {
                var match = levelUp.synergyList.FirstOrDefault(s =>
                    s.synergyEffect != null && s.synergyEffect.effectId == effectId);

                if (match != null && match.replaceOriginal)
                {
                    SuppressEffectsForSynergy(match.requiredEffectIds);
                }
            }
        }

        // 4) ��������
        if (target == null) target = FindObjectOfType<Player>();
        if (target != null) RefreshTargetEffects(target);

        
    }

    // ========== Ÿ���� ȿ�� ���ΰ�ħ ==========
    private void RefreshTargetEffects(IEffectTarget target)
    {
        // ���� ȿ�� ����
        RemoveAllEffectsFromTarget(target);

        // ������ ��� ȿ���� Ÿ�ٿ� ����
        foreach (var effectInfo in ownedEffects.Values)
        {
            if (effectInfo.isActive && !effectInfo.isSuppressedBySynergy)
            {
                ApplyEffectToTarget(target, effectInfo);
            }
        }
    }

    // ========== ���� ȿ�� ���� ==========
    private void ApplyEffectToTarget(IEffectTarget target, EffectInfo effectInfo)
    {
        // ȿ�� ���� (���� �ݿ�)
        var effectData = CreateLeveledEffectData(effectInfo);
        IEffect effect = EffectFactory.CreateEffect(effectData);

        if (effect == null) return;

        // Ÿ���� Ȱ�� ȿ�� ��Ͽ� �߰�
        if (!activeEffects.ContainsKey(target))
            activeEffects[target] = new List<IEffect>();

        activeEffects[target].Add(effect);

        // Ÿ�ٿ� ȿ�� ����
        target.ApplyEffect(effect);
        Debug.Log($"ȿ�� ����: {effectInfo.effectId} Lv.{effectInfo.level} �� {target}");
    }

    // ========== ������ �ݿ��� ȿ�� ������ ���� ==========
    private EffectData CreateLeveledEffectData(EffectInfo effectInfo)
    {
        var baseData = effectInfo.effectData;
        float leveledValue = baseData.value + (baseData.valuePerLevel * (effectInfo.level - 1));

        return new EffectData
        {
            effectType = baseData.effectType,
            value = leveledValue,
            specialId = baseData.specialId,
            parameters = baseData.parameters
        };
    }

    // ========== �ó��� üũ �� ���� (�ɼ�) ==========
    private void CheckAndApplySynergies()
    {
        // SynergyManager�� ��� �����ϵ��� ����
        // ���߿� SynergyManager�� �����ϸ� ���⼭ ����

        // ����� LevelUp�� synergyList�� ���� üũ
        LevelUp levelUp = FindObjectOfType<LevelUp>();
        if (levelUp != null && levelUp.synergyList != null)
        {
            foreach (var synergy in levelUp.synergyList)
            {
                if (synergy.synergyEffect == null) continue;
                if (HasEffect(synergy.synergyEffect.effectId)) continue;

                // �ʿ��� ȿ������ ��� �����ϰ� �ִ��� Ȯ��
                if (HasAllEffects(synergy.requiredEffectIds))
                {
                    // �ó��� ȿ�� �ڵ� ȹ��
                    AcquireEffect(synergy.synergyEffect);

                    // ���� ȿ�� ���� (�ɼ�)
                    if (synergy.replaceOriginal)
                    {
                        SuppressEffectsForSynergy(synergy.requiredEffectIds);
                    }

                    Debug.Log($"�ó��� �ߵ�: {synergy.synergyName}");
                }
            }
        }
    }

    // ========== ȿ�� ���� ==========
    private void RemoveAllEffectsFromTarget(IEffectTarget target)
    {
        if (!activeEffects.ContainsKey(target)) return;

        foreach (var effect in activeEffects[target])
        {
            target.RemoveEffect(effect);
        }
        activeEffects[target].Clear();
    }

    // ========== ��ȸ �޼���� ==========
    public bool HasEffect(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) && ownedEffects[effectId].isActive;
    }

    public bool HasAllEffects(List<string> effectIds)
    {
        return effectIds.All(id => HasEffect(id)); // System.Linq �ʿ�
    }

    public int GetEffectLevel(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) ? ownedEffects[effectId].level : 0;
    }

    public List<string> GetOwnedEffectIds()
    {
        return ownedEffects.Keys.ToList(); // System.Linq �ʿ�
    }

    public List<IEffect> GetTargetActiveEffects(IEffectTarget target)
    {
        return activeEffects.ContainsKey(target)
            ? new List<IEffect>(activeEffects[target])
            : new List<IEffect>();
    }

    public EffectInfo GetEffectInfo(string effectId)
    {
        return ownedEffects.ContainsKey(effectId) ? ownedEffects[effectId] : null;
    }

    // ========== �ó����� ���� ȿ�� ���� ==========
    public void SuppressEffectsForSynergy(List<string> effectIds)
    {
        foreach (string id in effectIds)
        {
            if (ownedEffects.ContainsKey(id))
            {
                ownedEffects[id].isSuppressedBySynergy = true;
            }
        }

        // ��� Ÿ�� ���ΰ�ħ
        foreach (var target in activeEffects.Keys.ToList()) // ToList()�� ���纻 ����
        {
            RefreshTargetEffects(target);
        }
    }

    // ========== �ʱ�ȭ ==========
    public void Reset()
    {
        foreach (var kvp in activeEffects)
        {
            RemoveAllEffectsFromTarget(kvp.Key);
        }

        ownedEffects.Clear();
        activeEffects.Clear();
        Debug.Log("EffectManager �ʱ�ȭ �Ϸ�");
    }

    // ========== ����/�ε� ==========
    public SaveData GetSaveData()
    {
        return new SaveData
        {
            ownedEffects = ownedEffects.Values.ToList() // System.Linq �ʿ�
        };
    }

    public void LoadSaveData(SaveData data)
    {
        ownedEffects.Clear();
        foreach (var info in data.ownedEffects)
        {
            ownedEffects[info.effectId] = info;
        }

        // �÷��̾�� ȿ�� ������
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            RefreshTargetEffects(player);
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public List<EffectInfo> ownedEffects;
    }
}

// ========== ȿ�� ���� Ŭ���� ==========
[System.Serializable]
public class EffectInfo
{
    public string effectId;
    public EffectDataScriptable effectData;
    public int level = 1;
    public bool isActive = true;
    public bool isSuppressedBySynergy = false; // �ó����� ���� ������
}