using System.Collections.Generic;
using UnityEngine;

public class EffectLevelManager : MonoBehaviour
{
    public static EffectLevelManager Instance { get; private set; }

    // 효과별 레벨 저장
    private Dictionary<string, int> effectLevels;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            effectLevels = new Dictionary<string, int>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetEffectLevel(string effectId)
    {
        if (effectLevels.ContainsKey(effectId))
            return effectLevels[effectId];
        return 0;
    }

    public void IncreaseEffectLevel(string effectId)
    {
        if (effectLevels.ContainsKey(effectId))
            effectLevels[effectId]++;
        else
            effectLevels[effectId] = 1;

        Debug.Log($"{effectId} 레벨: {effectLevels[effectId]}");
    }

    public void ResetLevels()
    {
        effectLevels.Clear();
    }

    public Dictionary<string, int> GetAllLevels()
    {
        return new Dictionary<string, int>(effectLevels);
    }
}