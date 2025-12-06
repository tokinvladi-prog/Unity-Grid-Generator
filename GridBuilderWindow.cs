using UnityEditor;
using UnityEngine;

public class GridBuilderWindow : EditorWindow
{
    // private GameObject _currentObject;
    // private int _rotationStep = 0;
    // private const string GEOMETRY_GAMEOBJECT_NAME = "Geometry";
    // private readonly Vector3 GRID_START = Vector3.zero;
    // private Vector2Int _lastPosition;

    // private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    // private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    // private void OnSceneGUI(SceneView sceneView)
    // {
    //     GameObject geometryStorage = GameObject.Find(GEOMETRY_GAMEOBJECT_NAME);
    //     if (!geometryStorage)
    //     {
    //         geometryStorage = new GameObject(GEOMETRY_GAMEOBJECT_NAME);
    //         geometryStorage.transform.position = GRID_START;
    //     }

    //     HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

    //     DrawGrid();

    //     switch (Event.current.type)
    //     {
    //         case EventType.DragUpdated:
    //             DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    //             if (_currentObject == null)
    //             {
    //                 GameObject draggedObject = DragAndDrop.objectReferences[0] as GameObject;
    //                 _currentObject = PrefabUtility.InstantiatePrefab(draggedObject) as GameObject;
    //                 _currentObject.gameObject.transform.SetParent(geometryStorage.transform);
    //             }
    //             HandleDrag();
    //             Event.current.Use();
    //             break;

    //         case EventType.DragPerform:
    //             if (_currentObject.TryGetComponent<Resource>(out var resource))
    //             {
    //                 _cellStorage.SetCellStatus(_lastPosition, CellStatus.Resource);
    //             }
    //             DragAndDrop.AcceptDrag();
    //             _currentObject = null;
    //             SceneView.RepaintAll();
    //             Event.current.Use();
    //             break;

    //         case EventType.MouseDown:
    //             if (Event.current.button == 0)
    //             {
    //                 GameObject clickedObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);

    //                 if (clickedObject != null)
    //                 {
    //                     if (clickedObject.transform.IsChildOf(geometryStorage.transform))
    //                     {
    //                         _currentObject = clickedObject;
    //                         Selection.activeGameObject = _currentObject;
    //                         Event.current.Use();
    //                     }
    //                     Event.current.Use();
    //                 }
    //             }
    //             break;

    //         case EventType.KeyDown:
    //             if (Event.current.keyCode == KeyCode.R && Event.current.control)
    //             {
    //                 if (_currentObject != null)
    //                 {
    //                     _rotationStep = (_rotationStep + 1) % 4;
    //                     float angle = _rotationStep * 90f;
    //                     _currentObject.transform.rotation = Quaternion.Euler(0, angle, 0);
    //                 }
    //                 Event.current.Use();
    //             }
    //             break;
    //     }
    // }

    // private void HandleDrag()
    // {
    //     if (_currentObject == null) return;

    //     Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    //     Plane plane = new(Vector3.up, GRID_START.y);
    //     if (plane.Raycast(ray, out float distance))
    //     {
    //         Vector3 hitPoint = ray.GetPoint(distance);
    //         Vector2Int cellCoordinate = WorldToGridCoordinate(hitPoint);

    //         _currentObject.transform.position = new Vector3(
    //             cellCoordinate.x * _cellSize + _cellSize / 2f,
    //             GRID_START.y,
    //             cellCoordinate.y * _cellSize + _cellSize / 2f);

    //         _lastPosition = cellCoordinate;
    //     }
    // }

    // private Vector2Int WorldToGridCoordinate(Vector3 worldPosition)
    // {
    //     float localX = worldPosition.x - GRID_START.x;
    //     float localZ = worldPosition.z - GRID_START.z;

    //     int x = Mathf.FloorToInt(localX / _cellSize);
    //     int y = Mathf.FloorToInt(localZ / _cellSize);

    //     return new Vector2Int(x, y);
    // }
}
