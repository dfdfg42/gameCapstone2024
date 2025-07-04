using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Transform nodeContainer;
    public Transform lineContainer;
    public Canvas mapCanvas;

    [Header("프리팹")]
    public GameObject nodeUIPrefab;
    public GameObject linePrefab;

    [Header("맵 설정")]
    public Vector2 mapSize = new Vector2(800, 600);
    public float nodeScale = 1f;

    [Header("스테이지 정보 UI")]
    public Text stageText;
    public Text stageDescText;

    private Dictionary<int, MapNodeUI> nodeUIMap = new Dictionary<int, MapNodeUI>();
    private List<GameObject> connectionLines = new List<GameObject>();

    void Start()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapGenerated += RefreshMapDisplay;
            RefreshMapDisplay();
        }
    }

    void OnDestroy()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapGenerated -= RefreshMapDisplay;
        }
    }

    public void RefreshMapDisplay()
    {
        if (MapManager.Instance == null || MapManager.Instance.currentMapData == null)
            return;

        ClearMap();
        DisplayMap();
        UpdateStageInfo();
    }

    private void ClearMap()
    {
        // 기존 노드들 제거
        foreach (var nodeUI in nodeUIMap.Values)
        {
            if (nodeUI != null && nodeUI.gameObject != null)
                Destroy(nodeUI.gameObject);
        }
        nodeUIMap.Clear();

        // 기존 연결선들 제거
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line);
        }
        connectionLines.Clear();
    }

    private void DisplayMap()
    {
        var mapData = MapManager.Instance.currentMapData;

        // 노드들 생성
        foreach (var node in mapData.nodes)
        {
            CreateNodeUI(node);
        }

        // 연결선들 생성
        foreach (var node in mapData.nodes)
        {
            CreateConnectionLines(node);
        }
    }

    private void CreateNodeUI(MapNode node)
    {
        if (nodeUIPrefab == null || nodeContainer == null)
        {
            Debug.LogError("Node UI Prefab 또는 Container가 설정되지 않았습니다!");
            return;
        }

        GameObject nodeObj = Instantiate(nodeUIPrefab, nodeContainer);
        MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();

        if (nodeUI == null)
        {
            nodeUI = nodeObj.AddComponent<MapNodeUI>();
        }

        // 노드 UI 초기화
        nodeUI.Initialize(node);

        // 위치 설정 (맵 좌표를 UI 좌표로 변환)
        Vector2 uiPosition = ConvertMapPositionToUI(node.position);
        nodeObj.GetComponent<RectTransform>().anchoredPosition = uiPosition;

        // 스케일 설정
        nodeObj.transform.localScale = Vector3.one * nodeScale;

        nodeUIMap[node.nodeId] = nodeUI;
    }

    private void CreateConnectionLines(MapNode node)
    {
        if (linePrefab == null || lineContainer == null)
            return;

        var nodeUI = nodeUIMap[node.nodeId];
        Vector2 startPos = nodeUI.GetComponent<RectTransform>().anchoredPosition;

        foreach (int connectedNodeId in node.connectedNodes)
        {
            if (nodeUIMap.ContainsKey(connectedNodeId))
            {
                var connectedNodeUI = nodeUIMap[connectedNodeId];
                Vector2 endPos = connectedNodeUI.GetComponent<RectTransform>().anchoredPosition;

                GameObject line = CreateLine(startPos, endPos);
                connectionLines.Add(line);
            }
        }
    }

    private GameObject CreateLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObj = Instantiate(linePrefab, lineContainer);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 선의 위치와 회전 계산
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 위치를 중점으로 설정
        lineRect.anchoredPosition = startPos + direction * 0.5f;
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
        lineRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return lineObj;
    }

    private Vector2 ConvertMapPositionToUI(Vector2 mapPosition)
    {
        // 맵 좌표계를 UI 좌표계로 변환
        // 맵의 중심을 (0,0)으로 하고 적절히 스케일링
        return mapPosition * 100f; // 100은 스케일 팩터
    }

    private void UpdateStageInfo()
    {
        if (MapManager.Instance == null) return;

        if (stageText != null)
        {
            stageText.text = $"Stage {MapManager.Instance.currentStage}";
        }

        if (stageDescText != null)
        {
            stageDescText.text = "목적지를 선택하세요";
        }
    }

    // 노드 상태 업데이트
    public void UpdateNodeStates()
    {
        foreach (var kvp in nodeUIMap)
        {
            var nodeUI = kvp.Value;
            nodeUI.UpdateNodeState();
        }
    }
}

// ========== 개별 노드 UI 클래스 ==========
public class MapNodeUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image nodeImage;
    public Text nodeText;
    public Button nodeButton;
    public GameObject selectedIndicator;
    public GameObject visitedIndicator;

    [Header("노드 타입별 색상")]
    public Color startColor = Color.green;
    public Color battleColor = Color.red;
    public Color restColor = Color.blue;
    public Color eventColor = Color.yellow;
    public Color treasureColor = new Color(1.0f, 0.64f, 0.0f); // 주황색 (RGB)
    public Color bossColor = new Color(0.5f, 0.0f, 0.5f);   // 보라색 (RGB)
    public Color disabledColor = Color.gray;

    private MapNode nodeData;
    private bool isInitialized = false;

    void Awake()
    {
        if (nodeButton == null)
            nodeButton = GetComponent<Button>();
        if (nodeImage == null)
            nodeImage = GetComponent<Image>();
        if (nodeText == null)
            nodeText = GetComponentInChildren<Text>();

        if (nodeButton != null)
            nodeButton.onClick.AddListener(OnNodeClicked);
    }

    public void Initialize(MapNode node)
    {
        nodeData = node;
        isInitialized = true;

        UpdateNodeAppearance();
        UpdateNodeState();
    }

    private void UpdateNodeAppearance()
    {
        if (!isInitialized) return;

        // 노드 텍스트 설정
        if (nodeText != null)
        {
            nodeText.text = GetNodeDisplayText();
        }

        // 노드 색상 설정
        if (nodeImage != null)
        {
            nodeImage.color = GetNodeColor();
        }
    }

    public void UpdateNodeState()
    {
        if (!isInitialized || MapManager.Instance == null) return;

        bool isAvailable = MapManager.Instance.IsNodeAvailable(nodeData.nodeId);
        bool isVisited = MapManager.Instance.IsNodeVisited(nodeData.nodeId);
        bool isCurrent = MapManager.Instance.GetCurrentNode()?.nodeId == nodeData.nodeId;

        // 버튼 활성화 상태
        if (nodeButton != null)
        {
            nodeButton.interactable = isAvailable && !isVisited;
        }

        // 선택 표시기
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isCurrent);
        }

        // 방문 표시기
        if (visitedIndicator != null)
        {
            visitedIndicator.SetActive(isVisited);
        }

        // 색상 업데이트
        if (nodeImage != null)
        {
            if (!isAvailable && !isVisited && !isCurrent)
            {
                nodeImage.color = disabledColor;
            }
            else
            {
                nodeImage.color = GetNodeColor();
            }
        }
    }

    private string GetNodeDisplayText()
    {
        switch (nodeData.nodeType)
        {
            case NodeType.Start: return "시작";
            case NodeType.Battle: return "⚔";
            case NodeType.Rest: return "💤";
            case NodeType.Event: return "?";
            case NodeType.Treasure: return "📦";
            case NodeType.Boss: return "👑";
            default: return "?";
        }
    }

    private Color GetNodeColor()
    {
        switch (nodeData.nodeType)
        {
            case NodeType.Start: return startColor;
            case NodeType.Battle: return battleColor;
            case NodeType.Rest: return restColor;
            case NodeType.Event: return eventColor;
            case NodeType.Treasure: return treasureColor;
            case NodeType.Boss: return bossColor;
            default: return Color.white;
        }
    }

    private void OnNodeClicked()
    {
        if (!isInitialized || MapManager.Instance == null) return;

        Debug.Log($"노드 클릭: {nodeData.nodeName} (ID: {nodeData.nodeId})");
        MapManager.Instance.SelectNode(nodeData.nodeId);
    }

    // 툴팁 표시용 (추후 구현)
    public void ShowTooltip()
    {
        if (isInitialized)
        {
            Debug.Log($"노드 정보: {nodeData.nodeName} - {nodeData.description}");
        }
    }
}