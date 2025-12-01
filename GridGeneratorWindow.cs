using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridGeneratorWindow : EditorWindow
{
    #region Fields

    private Cell _cell;
    private CellStorage _cellStorage;
    private GameObject _currentObject;
    //private int _rotationStep = 0;
    private float _cellSize = 1;
    private Vector2Int _gridSize = new(10, 10);
    private Vector2Int _lastPosition;

    private bool _drawGrid = false;
    private bool _drawName = false;

    //private const float ROTATION_STEP = 90;
    private const string GRID_GAMEOBJECT_NAME = "Cells Storage";
    private const string GEOMETRY_GAMEOBJECT_NAME = "Geometry";
    private readonly Vector3 GRID_START = Vector3.zero;

    #endregion

    [MenuItem("Tools/Open Grid Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Grid Generator");
    }

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;

    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        GUILayout.Label("Grid Generation Settings", EditorStyles.boldLabel);

        _cell = (Cell)EditorGUILayout.ObjectField("Cell Prefab", _cell, typeof(Cell), false);
        _cellStorage = (CellStorage)EditorGUILayout.ObjectField("Cell Storage Object", _cellStorage, typeof(CellStorage), false);

        _gridSize = Vector2Int.RoundToInt(EditorGUILayout.Vector2Field("Grid Size", _gridSize));
        _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize);

        _drawGrid = GUILayout.Toggle(_drawGrid, "Active Grid Draw");
        _drawName = GUILayout.Toggle(_drawName, "Active Name Draw");

        _cellStorage.GridSize = _gridSize;
        _cellStorage.CellSize = _cellSize;
        _cellStorage.GridStart = GRID_START;

        if (GUILayout.Button("Generate Grid")) GenerateGrid();
        if (GUILayout.Button("Destroy Grid")) ClearGrid();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        GameObject geometryStorage = GameObject.Find(GEOMETRY_GAMEOBJECT_NAME);
        if (!geometryStorage)
        {
            geometryStorage = new GameObject(GEOMETRY_GAMEOBJECT_NAME);
            geometryStorage.transform.position = GRID_START;
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        DrawGrid();

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (_currentObject == null)
                {
                    GameObject draggedObject = DragAndDrop.objectReferences[0] as GameObject;
                    _currentObject = PrefabUtility.InstantiatePrefab(draggedObject) as GameObject;
                    _currentObject.gameObject.transform.SetParent(geometryStorage.transform);
                }
                HandleDrag();
                Event.current.Use();
                break;

            case EventType.DragPerform:
                if (_currentObject.TryGetComponent<Resource>(out var resource))
                {
                    _cellStorage.SetCellStatus(_lastPosition, CellStatus.Resource);
                }
                DragAndDrop.AcceptDrag();
                _currentObject = null;
                SceneView.RepaintAll();
                Event.current.Use();
                break;

            case EventType.MouseDown:
                if (Event.current.button == 0)
                {
                    HandleClick();
                    Event.current.Use();
                }
                break;

            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.R && Event.current.control)
                {
                    Debug.Log("Rotate");
                    Event.current.Use();
                }
                break;
        }
    }

    private void HandleDrag()
    {
        if (_currentObject == null) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane plane = new(Vector3.up, GRID_START.y);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector2Int cellCoordinate = WorldToGridCoordinate(hitPoint);

            _currentObject.transform.position = new Vector3(
                cellCoordinate.x * _cellSize + _cellSize / 2f,
                GRID_START.y,
                cellCoordinate.y * _cellSize + _cellSize / 2f);

            _lastPosition = cellCoordinate;
        }
    }

    private void HandleClick()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("GameObject: " + hit.collider.gameObject.name);
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

    #region Grid Logic

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
                Vector2Int position = new(cell_x, cell_z);
                cell.Initialize(position);
                cell.Position = position;

                _cellStorage.Cells.Add(cell);
            }
        }

        Debug.Log($"<color=cyan>Generate grid with {_gridSize.x * _gridSize.y} cells</color>");
    }

    private void DrawGrid()
    {
        Color color = Color.cyan;

        if (_drawGrid)
        {
            UtilityShapesDrawer.DrawGrid(GRID_START, _cellSize, _gridSize, color);
        }

        GUIStyle labelStyle = new()
        {
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };

        if (_drawName)
        {
            UtilityShapesDrawer.DrawGridLabels(GRID_START, _cellSize, _gridSize, string.Empty/*"Cell\n"*/, labelStyle);
        }
    }

    private void ClearGrid()
    {
        GameObject cellsStorage = GameObject.Find(GRID_GAMEOBJECT_NAME);
        if (!cellsStorage) return;

        for (int i = cellsStorage.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(cellsStorage.transform.GetChild(i).gameObject);

        _cellStorage.Cells.Clear();
    }

    #endregion
}