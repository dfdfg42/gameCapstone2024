using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectInventory : MonoBehaviour
{
    public static EffectInventory Instance { get; private set; }

    private HashSet<string> ownedEffects; // ������ ȿ�� ID��

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ownedEffects = new HashSet<string>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEffect(string effectId)
    {
        ownedEffects.Add(effectId);
        Debug.Log($"ȿ�� ȹ��: {effectId}");
    }

    public bool HasEffect(string effectId)
    {
        return ownedEffects.Contains(effectId);
    }

    public List<string> GetAllEffects()
    {
        return ownedEffects.ToList();
    }

    public void Reset()
    {
        ownedEffects.Clear();
    }

    // �ó��� üũ
    public bool HasAllEffects(List<string> requiredEffects)
    {
        return requiredEffects.All(id => ownedEffects.Contains(id));
    }
}