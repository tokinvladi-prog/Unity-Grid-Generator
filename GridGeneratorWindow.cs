using UnityEditor;
using UnityEngine;

public class GridGeneratorWindow : EditorWindow
{
    private Cell _cell;
    private GameObject _draggedPrefab;
    private bool _isDragging = false;
    private Vector2Int _lastPosition;
    private int _rotationStep = 0;
    private float _cellSize = 1;
    private Vector2Int _gridSize = new(10, 10);

    private const float ROTATION_STEP = 90;
    private const string GRID_GAMEOBJECT_NAME = "Cells Storage";
    private readonly Vector3 GRID_START = Vector3.zero;


    [MenuItem("Tools/Open Grid Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Grid Generator");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Generation Settings", EditorStyles.boldLabel);

        _cell = (Cell)EditorGUILayout.ObjectField("Cell Prefab", _cell, typeof(Cell), false);

        _gridSize = Vector2Int.RoundToInt(EditorGUILayout.Vector2Field("Grid Size", _gridSize));
        _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize);

        if (GUILayout.Button("Generate Grid")) GenerateGrid();
        if (GUILayout.Button("Destroy Grid")) ClearGrid();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        DrawGrid();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R && Event.current.control)
        {
            Debug.Log("Rotate object!");
            _rotationStep = (_rotationStep + 1) % 4;
            SceneView.RepaintAll();
            Event.current.Use();
        }

        if (_isDragging && _draggedPrefab != null && _lastPosition != -Vector2Int.one)
        {
            DrawGhostObject(_lastPosition, _rotationStep);
        }

        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            HandleProjectWindowDrag();
        }

        if (Event.current.type == EventType.DragExited)
        {
            EndDrag();
        }
    }

    private void HandleDrag()
    {
        if (_draggedPrefab == null) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane plane = new(Vector3.up, GRID_START.y);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector2Int cellCoordinate = WorldToGridCoordinate(hitPoint);

            if (cellCoordinate != _lastPosition)
            {
                _lastPosition = cellCoordinate;
            }
        }
    }

    private Vector2Int WorldToGridCoordinate(Vector3 worldPosition)
    {
        float localX = worldPosition.x - GRID_START.x;
        float localZ = worldPosition.z - GRID_START.z;

        int x = Mathf.FloorToInt(localX / _cellSize);
        int y = Mathf.FloorToInt(localZ / _cellSize);

        return new Vector2Int(x, y);
    }

    private void EndDrag()
    {
        if (_isDragging && _draggedPrefab != null && _lastPosition != -Vector2Int.one)
        {
            InstantiateObject(_lastPosition);
        }

        _isDragging = false;
        _draggedPrefab = null;
        _lastPosition = -Vector2Int.one;
        _rotationStep = 0;

        SceneView.RepaintAll();
    }

    private void HandleProjectWindowDrag()
    {
        if (DragAndDrop.objectReferences.Length == 0 || !(DragAndDrop.objectReferences[0] is GameObject))
            return;

        GameObject draggedObject = DragAndDrop.objectReferences[0] as GameObject;

        if (PrefabUtility.GetPrefabAssetType(draggedObject) == PrefabAssetType.NotAPrefab)
            return;

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (!_isDragging)
                {
                    _isDragging = true;
                    _draggedPrefab = draggedObject;
                    _rotationStep = 0;
                }
                HandleDrag();
                Event.current.Use();
                break;

            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();
                EndDrag();
                Event.current.Use();
                break;
        }
    }

    private void InstantiateObject(Vector2Int cellCoordinate)
    {
        Vector3 worldPosition = new Vector3(
            cellCoordinate.x * _cellSize + _cellSize / 2f,
            GRID_START.y,
            cellCoordinate.y * _cellSize + _cellSize / 2f
        );

        float angle = _rotationStep * 90f;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        GameObject instance = PrefabUtility.InstantiatePrefab(_draggedPrefab) as GameObject;
        instance.transform.position = worldPosition;
        instance.transform.rotation = rotation;

        Debug.Log($"<color=green>Instantiate {instance.name} at cell {cellCoordinate} with rotation {angle}</color>");
    }

    private void DrawGhostObject(Vector2Int cellCoordinate, int rotationSteps)
    {
        Vector3 worldPosition = new(cellCoordinate.x * _cellSize + _cellSize / 2, GRID_START.y + _cellSize / 2, cellCoordinate.y * _cellSize + _cellSize / 2);
        Quaternion rotation = Quaternion.Euler(0, rotationSteps * ROTATION_STEP, 0);

        Handles.color = Color.green;
        Handles.DrawWireCube(worldPosition, _cellSize * Vector3.one);
        UtilityShapesDrawer.DrawArrow(worldPosition + new Vector3(0, _cellSize / 2, 0), rotation, _cellSize, 0.4f, Color.magenta);
    }

    private void GenerateGrid()
    {
        GameObject cellsStorage = GameObject.Find(GRID_GAMEOBJECT_NAME);
        if (!cellsStorage)
        {
            cellsStorage = new GameObject(GRID_GAMEOBJECT_NAME);
            cellsStorage.transform.position = GRID_START;
        }

        ClearGrid();

        for (int cell_x = 0; cell_x < _gridSize.x; cell_x++)
        {
            for (int cell_z = 0; cell_z < _gridSize.y; cell_z++)
            {
                GameObject cellObj = PrefabUtility.InstantiatePrefab(_cell.gameObject) as GameObject;
                Cell cell = cellObj.GetComponent<Cell>();
                cell.gameObject.transform.SetParent(cellsStorage.transform);
                cell.gameObject.transform.position = GRID_START + new Vector3(cell_x * _cellSize, 0, cell_z * _cellSize);
                cell.Initialize(new Vector2Int(cell_x, cell_z));
            }
        }

        Debug.Log($"<color=cyan>Generate grid with {_gridSize.x * _gridSize.y} cells</color>");
    }

    private void DrawGrid()
    {
        Color color = Color.cyan;

        UtilityShapesDrawer.DrawGrid(GRID_START, _cellSize, _gridSize, color);

        GUIStyle labelStyle = new GUIStyle()
        {
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };
        UtilityShapesDrawer.DrawGridLabels(GRID_START, _cellSize, _gridSize, string.Empty/*"Cell\n"*/, labelStyle);
    }

    private void ClearGrid()
    {
        GameObject cellsStorage = GameObject.Find(GRID_GAMEOBJECT_NAME);
        if (!cellsStorage) return;

        for (int i = cellsStorage.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(cellsStorage.transform.GetChild(i).gameObject);
    }
}