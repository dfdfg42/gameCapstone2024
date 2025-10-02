using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<string, List<IEffect>> activeEffectsByTarget;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        activeEffectsByTarget = new Dictionary<string, List<IEffect>>();
    }

    // ȿ�� ����
    public void ApplyEffect(IEffectTarget target, IEffect effect)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
        {
            activeEffectsByTarget[targetId] = new List<IEffect>();
        }

        var targetEffects = activeEffectsByTarget[targetId];

        // ���� ������ ȿ������ Ȯ��
        var existingEffect = targetEffects.FirstOrDefault(e => e.EffectId == effect.EffectId);
        if (existingEffect != null)
        {
            if (!effect.CanStackWith(existingEffect))
            {
                Debug.Log($"ȿ�� {effect.Name}�� �ߺ� ������ �� �����ϴ�.");
                return;
            }
        }

        // ȿ�� ����
        effect.Apply(target);
        targetEffects.Add(effect);

        Debug.Log($"{GetTargetName(target)}���� {effect.Name} ȿ�� ����");
    }

    // ȿ�� ����
    public void RemoveEffect(IEffectTarget target, string effectId)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
            return;

        var targetEffects = activeEffectsByTarget[targetId];
        var effectToRemove = targetEffects.FirstOrDefault(e => e.EffectId == effectId);

        if (effectToRemove != null)
        {
            effectToRemove.Remove(target);
            targetEffects.Remove(effectToRemove);

            Debug.Log($"{GetTargetName(target)}���� {effectToRemove.Name} ȿ�� ����");
        }
    }

    // ��� ȿ�� ����
    public void RemoveAllEffects(IEffectTarget target)
    {
        string targetId = GetTargetId(target);

        if (!activeEffectsByTarget.ContainsKey(targetId))
            return;

        var targetEffects = activeEffectsByTarget[targetId];

        foreach (var effect in targetEffects.ToList())
        {
            effect.Remove(target);
        }

        targetEffects.Clear();
        Debug.Log($"{GetTargetName(target)}�� ��� ȿ�� ����");
    }

    // Ÿ���� ���� ID ��ȯ
    private string GetTargetId(IEffectTarget target)
    {
        if (target is MonoBehaviour mono)
            return mono.GetInstanceID().ToString();
        return target.GetHashCode().ToString();
    }

    // Ÿ���� �̸� ��ȯ
    private string GetTargetName(IEffectTarget target)
    {
        if (target is MonoBehaviour mono)
            return mono.name;
        return target.GetType().Name;
    }

    // ���� Ȱ��ȭ�� ȿ�� ��� ��ȯ
    public List<IEffect> GetActiveEffects(IEffectTarget target)
    {
        string targetId = GetTargetId(target);

        if (activeEffectsByTarget.ContainsKey(targetId))
            return new List<IEffect>(activeEffectsByTarget[targetId]);

        return new List<IEffect>();
    }
}