using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectInventory : MonoBehaviour
{
    public static EffectInventory Instance { get; private set; }

    private HashSet<string> ownedEffects; // 보유한 효과 ID들

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
        Debug.Log($"효과 획득: {effectId}");
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

    // 시너지 체크
    public bool HasAllEffects(List<string> requiredEffects)
    {
        return requiredEffects.All(id => ownedEffects.Contains(id));
    }
}