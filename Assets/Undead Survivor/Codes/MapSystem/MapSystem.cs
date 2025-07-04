using System.Collections.Generic;
using UnityEngine;

// ========== 노드 타입 열거형 ==========
public enum NodeType
{
    Start,      // 시작 노드
    Rest,       // 휴식
    Battle,     // 전투
    Event,      // 이벤트 (?)
    Treasure,   // 상자 (아이템)
    Boss        // 보스
}

// ========== 노드 데이터 클래스 ==========
[System.Serializable]
public class MapNode
{
    public int nodeId;
    public NodeType nodeType;
    public Vector2 position;
    public List<int> connectedNodes = new List<int>(); // 연결된 노드 ID들
    public bool isVisited = false;
    public bool isAvailable = false; // 현재 접근 가능한지

    [Header("노드별 데이터")]
    public string sceneName; // 해당 노드의 씬 이름
    public string nodeName;
    public string description;

    public MapNode(int id, NodeType type, Vector2 pos)
    {
        nodeId = id;
        nodeType = type;
        position = pos;
        isVisited = false;
        isAvailable = false;
    }
}

// ========== 맵 데이터 ScriptableObject ==========
[CreateAssetMenu(fileName = "New Map Data", menuName = "Map System/Map Data")]
public class MapData : ScriptableObject
{
    [Header("맵 설정")]
    public int stageNumber = 1;
    public string stageName = "Stage 1";
    public int layers = 6; // 맵의 층 수 (시작, 중간층들, 보스)
    public int nodesPerLayer = 3; // 각 층당 노드 개수

    [Header("노드 리스트")]
    public List<MapNode> nodes = new List<MapNode>();

    [Header("씬 정보")]
    public string restSceneName = "RestScene";
    public string battleSceneName = "BattleScene";
    public string eventSceneName = "EventScene";
    public string treasureSceneName = "TreasureScene";
    public string bossSceneName = "BossScene";
    public string mapSceneName = "MapScene";

    public MapNode GetNode(int nodeId)
    {
        return nodes.Find(node => node.nodeId == nodeId);
    }

    public List<MapNode> GetNodesOfType(NodeType type)
    {
        return nodes.FindAll(node => node.nodeType == type);
    }

    public List<MapNode> GetConnectedNodes(int nodeId)
    {
        var node = GetNode(nodeId);
        if (node == null) return new List<MapNode>();

        List<MapNode> connectedNodes = new List<MapNode>();
        foreach (int connectedId in node.connectedNodes)
        {
            var connectedNode = GetNode(connectedId);
            if (connectedNode != null)
                connectedNodes.Add(connectedNode);
        }
        return connectedNodes;
    }
}

// ========== 맵 진행 상태 관리 ==========
[System.Serializable]
public class MapProgress
{
    public int currentStage = 1;
    public int currentNodeId = -1;
    public List<int> visitedNodes = new List<int>();
    public List<int> availableNodes = new List<int>();
    public bool stageCompleted = false;

    public void VisitNode(int nodeId)
    {
        if (!visitedNodes.Contains(nodeId))
            visitedNodes.Add(nodeId);
        currentNodeId = nodeId;
    }

    public void SetAvailableNodes(List<int> nodeIds)
    {
        availableNodes.Clear();
        availableNodes.AddRange(nodeIds);
    }

    public bool IsNodeVisited(int nodeId)
    {
        return visitedNodes.Contains(nodeId);
    }

    public bool IsNodeAvailable(int nodeId)
    {
        return availableNodes.Contains(nodeId);
    }
}