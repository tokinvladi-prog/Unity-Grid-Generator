using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridBuilderWindow : EditorWindow
{
    #region Fields

    private string _folderPath = "Assets/Prefabs";
    private int _gridSize = 100;
    private int _rotationStep = 0;

    private Vector2 _scrollPosition;
    private Vector2Int _lastPosition;
    private Vector2Int _previousCellPosition;
    private List<GameObject> _prefabs = new();
    private GameObject _draggedPrefab;
    private GameObject _currentObject;

    #endregion

    [MenuItem("Tools/Open Grid Builder Window")]
    public static void ShowWindow()
    {
        GetWindow<GridBuilderWindow>("Grid Builder");
    }

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;

    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        DrawControls();
        DrawPrefabs();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (_currentObject == null)
                {
                    GameObject draggedObject = DragAndDrop.objectReferences[0] as GameObject;
                    _currentObject = PrefabUtility.InstantiatePrefab(draggedObject) as GameObject;
                }
                HandleDrag();
                Event.current.Use();
                break;

            case EventType.DragPerform:
                var cell = CellStorage.Instance.GetCell(_lastPosition);
                if (cell != null && cell.AttachedObject == null)
                {
                    cell.AttachedObject = _currentObject;

                    if (_currentObject.TryGetComponent<Resource>(out _))
                    {
                        CellStorage.Instance.SetCellStatus(cell, CellStatus.Resource);
                    }
                    DragAndDrop.AcceptDrag();
                }
                else
                {
                    DestroyImmediate(_currentObject);
                }
                _currentObject = null;

                SceneView.RepaintAll();
                Event.current.Use();
                break;

            case EventType.MouseDown:
                if (Event.current.button == 0)
                {
                    GameObject clickedObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);

                    if (clickedObject != null)
                    {
                        _currentObject = clickedObject;
                        Selection.activeGameObject = _currentObject;
                        _previousCellPosition = WorldToGridCoordinate(_currentObject.transform.position);

                        Cell previousCell = CellStorage.Instance.GetCell(_previousCellPosition);
                        if (previousCell != null && previousCell.AttachedObject == _currentObject)
                        {
                            previousCell.AttachedObject = null;
                        }

                        Event.current.Use();
                    }
                }
                break;

            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.R && Event.current.control)
                {
                    if (_currentObject != null)
                    {
                        _rotationStep = (_rotationStep + 1) % 4;
                        float angle = _rotationStep * 90f;
                        _currentObject.transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (_currentObject != null && Event.current.button == 0)
                {
                    HandleDrag();
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (_currentObject != null && Event.current.button == 0)
                {
                    CellStorage.Instance.SetCellStatus(_previousCellPosition, CellStatus.Empty);
                    Cell targetCell = CellStorage.Instance.GetCell(_lastPosition);

                    if (targetCell != null && targetCell.AttachedObject == null)
                    {
                        targetCell.AttachedObject = _currentObject;

                        if (_currentObject.TryGetComponent<Resource>(out _))
                        {
                            CellStorage.Instance.SetCellStatus(_lastPosition, CellStatus.Resource);
                        }
                    }
                    else if (targetCell != null && targetCell.AttachedObject != null)
                    {
                        _currentObject.transform.position = new Vector3(
                            _previousCellPosition.x * CellStorage.Instance.CellSize + CellStorage.Instance.CellSize / 2f,
                            CellStorage.Instance.GridStart.y,
                            _previousCellPosition.y * CellStorage.Instance.CellSize + CellStorage.Instance.CellSize / 2f);

                        Cell previousCell = CellStorage.Instance.GetCell(_previousCellPosition);
                        if (previousCell != null)
                        {
                            previousCell.AttachedObject = _currentObject;

                            if (_currentObject.TryGetComponent<Resource>(out _))
                            {
                                CellStorage.Instance.SetCellStatus(_previousCellPosition, CellStatus.Resource);
                            }
                        }
                    }
                    _currentObject = null;

                    SceneView.RepaintAll();
                    Event.current.Use();
                }
                break;
        }
    }

    private void HandleDrag()
    {
        if (_currentObject == null) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane plane = new(Vector3.up, CellStorage.Instance.GridStart.y);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector2Int cellCoordinate = WorldToGridCoordinate(hitPoint);

            _currentObject.transform.position = new Vector3(
                cellCoordinate.x * CellStorage.Instance.CellSize + CellStorage.Instance.CellSize / 2f,
                CellStorage.Instance.GridStart.y,
                cellCoordinate.y * CellStorage.Instance.CellSize + CellStorage.Instance.CellSize / 2f);
            _lastPosition = cellCoordinate;
        }
    }

    private Vector2Int WorldToGridCoordinate(Vector3 worldPosition)
    {
        float localX = worldPosition.x - CellStorage.Instance.GridStart.x;
        float localZ = worldPosition.z - CellStorage.Instance.GridStart.z;

        int x = Mathf.FloorToInt(localX / CellStorage.Instance.CellSize);
        int y = Mathf.FloorToInt(localZ / CellStorage.Instance.CellSize);

        return new Vector2Int(x, y);
    }

    #region GUI Prefabs Logic

    private void DrawControls()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Folder Path:", GUILayout.Width(80));
        _folderPath = EditorGUILayout.TextField(_folderPath);

        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                _folderPath = "Assets" + path[Application.dataPath.Length..];
            }
        }

        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            LoadPrefabs();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawPrefabs()
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        int columns = Mathf.Max(1, Mathf.FloorToInt(position.width / _gridSize));
        int rows = Mathf.CeilToInt(_prefabs.Count / (float)columns);

        for (int row = 0; row < rows; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;
                if (index >= _prefabs.Count) break;

                DrawPrefabItem(_prefabs[index]);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void LoadPrefabs()
    {
        _prefabs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { _folderPath });
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                _prefabs.Add(prefab);
            }
        }
    }

    private void DrawPrefabItem(GameObject prefab)
    {
        GUILayout.BeginVertical(GUILayout.Width(_gridSize), GUILayout.Height(_gridSize + 20));

        Rect previewRect = GUILayoutUtility.GetRect(_gridSize, _gridSize);
        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
        if (preview != null)
        {
            GUI.DrawTexture(previewRect, preview);
            Event ec = Event.current;

            if (ec.type == EventType.MouseDown && previewRect.Contains(ec.mousePosition))
            {
                _draggedPrefab = prefab;
                ec.Use();
            }

            if (ec.type == EventType.MouseDrag && _draggedPrefab == prefab)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { prefab };
                DragAndDrop.StartDrag("Dragging " + prefab.name);
                ec.Use();
            }
        }

        string displayName = prefab.name;
        if (displayName.Length > 15) displayName = displayName[..12] + "...";
        GUILayout.Label(displayName, EditorStyles.miniLabel, GUILayout.Width(_gridSize - 10));

        GUILayout.EndVertical();
    }

    #endregion
}
