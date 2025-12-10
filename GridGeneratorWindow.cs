using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridGeneratorWindow : EditorWindow
{
    #region Fields

    private Cell _cell;
    private Vector2Int _gridSize = new(10, 10);
    private Transform _container;

    private float _cellSize = 1;

    private bool _drawGrid = false;
    private bool _drawLabel = false;


    private const string GRID_GAMEOBJECT_NAME = "Cells Storage";
    private readonly Vector3 GRID_START = Vector3.zero;

    #endregion

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    [MenuItem("Tools/Open Grid Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Grid Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Generation Settings", EditorStyles.boldLabel);
        _container = GetGridContainer();

        _cell = (Cell)EditorGUILayout.ObjectField("Cell Prefab", _cell, typeof(Cell), false);

        _gridSize = Vector2Int.RoundToInt(EditorGUILayout.Vector2Field("Grid Size", _gridSize));
        _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize);

        _drawGrid = GUILayout.Toggle(_drawGrid, "Active Grid Draw");
        _drawLabel = GUILayout.Toggle(_drawLabel, "Active Name Draw");
        CellStorage.Instance.VisualizeCellsStatus = GUILayout.Toggle(CellStorage.Instance.VisualizeCellsStatus, "Active Status");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Grid", GUILayout.Width(100))) GenerateGrid();
        if (GUILayout.Button("Destroy Grid", GUILayout.Width(100))) ClearGrid(_container);
        GUILayout.EndHorizontal();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        DrawGrid();
        DrawLabels();
    }

    #region Grid Logic

    private void GenerateGrid()
    {
        ClearGrid(_container);

        for (int x = 0; x < _gridSize.x; x++)
            for (int y = 0; y < _gridSize.y; y++)
                CreateCell(x, y, _container);

        Debug.Log($"<color=cyan>Generate grid with {_gridSize.x * _gridSize.y} cells</color>");

        CellStorage.Instance.GridSize = _gridSize;
        CellStorage.Instance.CellSize = _cellSize;
        CellStorage.Instance.GridStart = GRID_START;
    }

    private Transform GetGridContainer()
    {
        var container = GameObject.Find(GRID_GAMEOBJECT_NAME);

        if (container == null)
        {
            container = new GameObject(GRID_GAMEOBJECT_NAME);
            container.transform.position = GRID_START;
        }

        return container.transform;
    }

    private void CreateCell(int x, int y, Transform parent)
    {
        if (!_cell) return;

        GameObject cellObject = PrefabUtility.InstantiatePrefab(_cell.gameObject, parent) as GameObject;

        if (cellObject.TryGetComponent(out Cell cell))
        {
            Vector2Int position = new(x, y);

            cell.gameObject.transform.position = GRID_START + new Vector3(x * _cellSize, 0, y * _cellSize);
            cell.Initialize(position);

            CellStorage.Instance.Cells.Add(cell);
        }
    }

    private void ClearGrid(Transform container)
    {
        container.Cast<Transform>().ToList().ForEach(c => DestroyImmediate(c.gameObject));
        CellStorage.Instance.Cells.Clear();
    }

    #endregion

    #region Draw Logic

    private void DrawGrid()
    {
        if (_drawGrid)
        {
            UtilityShapesDrawer.DrawGrid(GRID_START, _cellSize, _gridSize, Color.cyan);
        }
        
        SceneView.RepaintAll();
    }

    private void DrawLabels()
    {
        GUIStyle labelStyle = new()
        {
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };

        if (_drawLabel)
        {
            UtilityShapesDrawer.DrawGridLabels(GRID_START, _cellSize, _gridSize, string.Empty, labelStyle);
        }

        SceneView.RepaintAll();
    }

    #endregion
}