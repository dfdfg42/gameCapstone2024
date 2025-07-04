using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("맵 시스템 설정")]
    public MapGenerator mapGenerator;
    public MapData currentMapData;
    public MapProgress mapProgress;

    [Header("현재 상태")]
    public int currentStage = 1;
    public bool isInMapScene = true;

    // 이벤트
    public System.Action<MapNode> OnNodeSelected;
    public System.Action<int> OnStageCompleted;
    public System.Action OnMapGenerated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMapSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 첫 번째 스테이지 시작
        StartStage(currentStage);
    }

    private void InitializeMapSystem()
    {
        if (mapGenerator == null)
            mapGenerator = GetComponent<MapGenerator>();

        mapProgress = new MapProgress();
    }

    // ========== 스테이지 관리 ==========
    public void StartStage(int stageNumber)
    {
        currentStage = stageNumber;
        GenerateMap(stageNumber);

        // 시작 노드 설정
        var startNode = currentMapData.GetNodesOfType(NodeType.Start)[0];
        mapProgress = new MapProgress();
        mapProgress.currentStage = stageNumber;
        mapProgress.currentNodeId = startNode.nodeId;

        // 시작 노드에서 갈 수 있는 노드들 활성화
        UpdateAvailableNodes();

        Debug.Log($"Stage {stageNumber} 시작!");
        OnMapGenerated?.Invoke();
    }

    public void CompleteStage()
    {
        mapProgress.stageCompleted = true;
        Debug.Log($"Stage {currentStage} 완료!");
        OnStageCompleted?.Invoke(currentStage);

        // 다음 스테이지로 이동하거나 게임 완료 처리
        currentStage++;
        StartStage(currentStage);
    }

    // ========== 맵 생성 ==========
    private void GenerateMap(int stageNumber)
    {
        currentMapData = mapGenerator.GenerateMap(stageNumber);

        if (!mapGenerator.ValidateMap(currentMapData))
        {
            Debug.LogError("생성된 맵이 유효하지 않습니다!");
            return;
        }

        Debug.Log($"Stage {stageNumber} 맵 생성 완료 - 노드 수: {currentMapData.nodes.Count}");
    }

    // ========== 노드 이동 ==========
    public void SelectNode(int nodeId)
    {
        var node = currentMapData.GetNode(nodeId);
        if (node == null)
        {
            Debug.LogError($"노드 {nodeId}를 찾을 수 없습니다!");
            return;
        }

        if (!mapProgress.IsNodeAvailable(nodeId))
        {
            Debug.LogWarning($"노드 {nodeId}에 접근할 수 없습니다!");
            return;
        }

        // 노드 방문 처리
        VisitNode(node);

        // 해당 노드의 씬으로 이동
        TransitionToNodeScene(node);
    }

    private void VisitNode(MapNode node)
    {
        mapProgress.VisitNode(node.nodeId);
        node.isVisited = true;

        Debug.Log($"노드 방문: {node.nodeName} ({node.nodeType})");
        OnNodeSelected?.Invoke(node);

        // 보스 노드면 스테이지 완료 체크
        if (node.nodeType == NodeType.Boss)
        {
            // 보스 전투 후에 CompleteStage() 호출하도록 할 예정
        }
        else
        {
            // 다음에 갈 수 있는 노드들 업데이트
            UpdateAvailableNodes();
        }
    }

    private void UpdateAvailableNodes()
    {
        var currentNode = currentMapData.GetNode(mapProgress.currentNodeId);
        if (currentNode == null) return;

        List<int> availableNodeIds = new List<int>();

        // 현재 노드에서 연결된 노드들을 접근 가능하게 설정
        foreach (int connectedNodeId in currentNode.connectedNodes)
        {
            var connectedNode = currentMapData.GetNode(connectedNodeId);
            if (connectedNode != null && !connectedNode.isVisited)
            {
                availableNodeIds.Add(connectedNodeId);
                connectedNode.isAvailable = true;
            }
        }

        mapProgress.SetAvailableNodes(availableNodeIds);
        Debug.Log($"접근 가능한 노드 수: {availableNodeIds.Count}");
    }

    // ========== 씬 전환 ==========
    private void TransitionToNodeScene(MapNode node)
    {
        isInMapScene = false;

        string sceneName = GetSceneNameForNode(node);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"노드 {node.nodeType}에 대한 씬 이름이 설정되지 않았습니다!");
            return;
        }

        Debug.Log($"씬 전환: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private string GetSceneNameForNode(MapNode node)
    {
        if (!string.IsNullOrEmpty(node.sceneName))
            return node.sceneName;

        // 기본 씬 이름 설정
        switch (node.nodeType)
        {
            case NodeType.Battle:
                return "GameScene"; // 기존 게임 씬 사용
            case NodeType.Rest:
                return currentMapData.restSceneName;
            case NodeType.Event:
                return currentMapData.eventSceneName;
            case NodeType.Treasure:
                return currentMapData.treasureSceneName;
            case NodeType.Boss:
                return "GameScene"; // 보스도 기존 게임 씬 사용하되 특별 설정
            default:
                return currentMapData.mapSceneName;
        }
    }

    // ========== 맵으로 돌아가기 ==========
    public void ReturnToMap()
    {
        isInMapScene = true;
        SceneManager.LoadScene(currentMapData.mapSceneName);
    }

    // ========== 노드 완료 처리 ==========
    public void CompleteCurrentNode()
    {
        var currentNode = currentMapData.GetNode(mapProgress.currentNodeId);
        if (currentNode == null) return;

        if (currentNode.nodeType == NodeType.Boss)
        {
            // 보스 클리어시 스테이지 완료
            CompleteStage();
        }
        else
        {
            // 일반 노드 완료시 맵으로 돌아가기
            ReturnToMap();
        }
    }

    // ========== 게터 메서드들 ==========
    public MapNode GetCurrentNode()
    {
        return currentMapData?.GetNode(mapProgress.currentNodeId);
    }

    public List<MapNode> GetAvailableNodes()
    {
        List<MapNode> availableNodes = new List<MapNode>();
        foreach (int nodeId in mapProgress.availableNodes)
        {
            var node = currentMapData.GetNode(nodeId);
            if (node != null)
                availableNodes.Add(node);
        }
        return availableNodes;
    }

    public bool IsNodeAvailable(int nodeId)
    {
        return mapProgress.IsNodeAvailable(nodeId);
    }

    public bool IsNodeVisited(int nodeId)
    {
        return mapProgress.IsNodeVisited(nodeId);
    }

    // ========== 저장/로드 (추후 구현) ==========
    public void SaveMapProgress()
    {
        // PlayerPrefs나 파일로 진행상황 저장
        string progressJson = JsonUtility.ToJson(mapProgress);
        PlayerPrefs.SetString("MapProgress", progressJson);
        PlayerPrefs.SetInt("CurrentStage", currentStage);
    }

    public void LoadMapProgress()
    {
        if (PlayerPrefs.HasKey("MapProgress"))
        {
            string progressJson = PlayerPrefs.GetString("MapProgress");
            mapProgress = JsonUtility.FromJson<MapProgress>(progressJson);
            currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        }
    }
}