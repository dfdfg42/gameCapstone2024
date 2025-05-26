using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ========== 유물 매니저 (MonoBehaviour) ==========
public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    private List<Relic> ownedRelics;
    private IEffectTarget player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ownedRelics = new List<Relic>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 플레이어 참조 설정
        var playerObj = FindObjectOfType<Player>();
        if (playerObj != null)
            player = playerObj as IEffectTarget;
    }

    // 유물 획득
    public void AcquireRelic(string relicId)
    {
        var relicData = EffectManager.Instance.relicDatabase.FirstOrDefault(r => r.relicId == relicId);
        if (relicData == null)
        {
            Debug.LogError($"유물 데이터를 찾을 수 없습니다: {relicId}");
            return;
        }

        var relic = new Relic(relicData);
        ownedRelics.Add(relic);

        // 플레이어에게 유물 효과 적용
        if (player != null)
        {
            EffectManager.Instance.ApplyEffect(player, relic);
        }

        Debug.Log($"유물 획득: {relic.Name}");

        // UI 업데이트 이벤트 발생
        OnRelicAcquired?.Invoke(relic);
    }

    // 유물 제거 (특수한 경우에만 사용)
    public void RemoveRelic(string relicId)
    {
        var relic = ownedRelics.FirstOrDefault(r => r.EffectId == relicId);
        if (relic != null)
        {
            ownedRelics.Remove(relic);

            if (player != null)
            {
                EffectManager.Instance.RemoveEffect(player, relicId);
            }

            Debug.Log($"유물 제거: {relic.Name}");
        }
    }

    // 소유한 유물 목록 반환
    public List<Relic> GetOwnedRelics()
    {
        return new List<Relic>(ownedRelics);
    }

    // 특정 유물 소유 여부 확인
    public bool HasRelic(string relicId)
    {
        return ownedRelics.Any(r => r.EffectId == relicId);
    }

    // 유물 획득 이벤트
    public event System.Action<Relic> OnRelicAcquired;

    // 랜덤 유물 획득 (테스트용)
    public void AcquireRandomRelic()
    {
        if (EffectManager.Instance.relicDatabase.Count > 0)
        {
            var randomRelic = EffectManager.Instance.relicDatabase[
                UnityEngine.Random.Range(0, EffectManager.Instance.relicDatabase.Count)];
            AcquireRelic(randomRelic.relicId);
        }
    }
}