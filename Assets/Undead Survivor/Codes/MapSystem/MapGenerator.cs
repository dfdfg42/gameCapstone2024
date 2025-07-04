using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("맵 생성 설정")]
    public int layers = 6;
    public int minNodesPerLayer = 3;
    public int maxNodesPerLayer = 4;
    public float layerSpacing = 2f;
    public float nodeSpacing = 1.5f;

    [Header("노드 타입 확률 (중간층)")]
    [Range(0f, 1f)] public float battleChance = 0.6f;
    [Range(0f, 1f)] public float restChance = 0.15f;
    [Range(0f, 1f)] public float eventChance = 0.15f;
    [Range(0f, 1f)] public float treasureChance = 0.1f;

    public MapData GenerateMap(int stageNumber)
    {
        MapData mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.stageNumber = stageNumber;
        mapData.stageName = $"Stage {stageNumber}";
        mapData.layers = layers;
        mapData.nodes = new List<MapNode>();

        int currentNodeId = 0;
        List<List<int>> layerNodes = new List<List<int>>();

        // 각 층별로 노드 생성
        for (int layer = 0; layer < layers; layer++)
        {
            List<int> currentLayerNodes = new List<int>();

            if (layer == 0) // 시작 노드
            {
                currentLayerNodes.Add(CreateStartNode(currentNodeId++, layer, mapData));
            }
            else if (layer == layers - 1) // 보스 노드
            {
                currentLayerNodes.Add(CreateBossNode(currentNodeId++, layer, mapData));
            }
            else // 중간층
            {
                int nodeCount = Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);
                for (int i = 0; i < nodeCount; i++)
                {
                    currentLayerNodes.Add(CreateRandomNode(currentNodeId++, layer, i, nodeCount, mapData));
                }
            }

            layerNodes.Add(currentLayerNodes);
        }

        // 노드 연결 생성
        ConnectLayers(layerNodes, mapData);

        return mapData;
    }

    private int CreateStartNode(int nodeId, int layer, MapData mapData)
    {
        Vector2 position = new Vector2(0, layer * layerSpacing);
        MapNode node = new MapNode(nodeId, NodeType.Start, position);
        node.sceneName = "MapScene"; // 시작은 맵 씬 유지
        node.nodeName = "시작";
        node.description = "여정의 시작";
        node.isAvailable = true;

        mapData.nodes.Add(node);
        return nodeId;
    }

    private int CreateBossNode(int nodeId, int layer, MapData mapData)
    {
        Vector2 position = new Vector2(0, layer * layerSpacing);
        MapNode node = new MapNode(nodeId, NodeType.Boss, position);
        node.sceneName = "BossScene";
        node.nodeName = "보스";
        node.description = "강력한 보스가 기다리고 있다";

        mapData.nodes.Add(node);
        return nodeId;
    }

    private int CreateRandomNode(int nodeId, int layer, int nodeIndex, int totalNodes, MapData mapData)
    {
        // 노드 위치 계산 (층에서 균등 분포)
        float xOffset = 0;
        if (totalNodes > 1)
        {
            float spacing = (totalNodes - 1) * nodeSpacing;
            xOffset = -spacing / 2 + nodeIndex * nodeSpacing;
        }
        Vector2 position = new Vector2(xOffset, layer * layerSpacing);

        // 노드 타입 랜덤 결정
        NodeType nodeType = DetermineRandomNodeType();
        MapNode node = new MapNode(nodeId, nodeType, position);

        // 노드별 정보 설정
        SetupNodeInfo(node);

        mapData.nodes.Add(node);
        return nodeId;
    }

    private NodeType DetermineRandomNodeType()
    {
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;

        cumulative += battleChance;
        if (random < cumulative) return NodeType.Battle;

        cumulative += restChance;
        if (random < cumulative) return NodeType.Rest;

        cumulative += eventChance;
        if (random < cumulative) return NodeType.Event;

        return NodeType.Treasure;
    }

    private void SetupNodeInfo(MapNode node)
    {
        switch (node.nodeType)
        {
            case NodeType.Battle:
                node.sceneName = "BattleScene";
                node.nodeName = "전투";
                node.description = "적들과 전투를 벌인다";
                break;

            case NodeType.Rest:
                node.sceneName = "RestScene";
                node.nodeName = "휴식";
                node.description = "체력을 회복한다";
                break;

            case NodeType.Event:
                node.sceneName = "EventScene";
                node.nodeName = "?";
                node.description = "무엇이 일어날지 모른다";
                break;

            case NodeType.Treasure:
                node.sceneName = "TreasureScene";
                node.nodeName = "보물";
                node.description = "유용한 아이템을 얻을 수 있다";
                break;
        }
    }

    private void ConnectLayers(List<List<int>> layerNodes, MapData mapData)
    {
        for (int layer = 0; layer < layerNodes.Count - 1; layer++)
        {
            List<int> currentLayer = layerNodes[layer];
            List<int> nextLayer = layerNodes[layer + 1];

            // 각 노드를 다음 층의 노드들과 연결
            foreach (int currentNodeId in currentLayer)
            {
                MapNode currentNode = mapData.GetNode(currentNodeId);

                // 연결 규칙: 각 노드는 다음 층의 1-3개 노드와 연결
                List<int> shuffledNextLayer = new List<int>(nextLayer);
                ShuffleList(shuffledNextLayer);

                int connectionsCount = Mathf.Min(Random.Range(1, 4), shuffledNextLayer.Count);
                for (int i = 0; i < connectionsCount; i++)
                {
                    if (!currentNode.connectedNodes.Contains(shuffledNextLayer[i]))
                    {
                        currentNode.connectedNodes.Add(shuffledNextLayer[i]);
                    }
                }
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // 맵 유효성 검사
    public bool ValidateMap(MapData mapData)
    {
        // 시작 노드가 있는지 확인
        var startNodes = mapData.GetNodesOfType(NodeType.Start);
        if (startNodes.Count != 1) return false;

        // 보스 노드가 있는지 확인  
        var bossNodes = mapData.GetNodesOfType(NodeType.Boss);
        if (bossNodes.Count != 1) return false;

        // 모든 노드가 연결되어 있는지 확인 (간단한 체크)
        foreach (var node in mapData.nodes)
        {
            if (node.nodeType != NodeType.Boss && node.connectedNodes.Count == 0)
                return false;
        }

        return true;
    }
}