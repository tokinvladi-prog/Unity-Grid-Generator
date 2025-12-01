using System;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using Zenject;

public enum CellStatus
{
    Empty,
    Block,
    Resource
}

public class Cell : MonoBehaviour
{
    public Vector2Int Position;
    public CellStatus Status = CellStatus.Empty;
    private MobileInput _input;
    private BuildingPlacer _buildingPlacer;

    [Inject]
    public void Construct(MobileInput input, BuildingPlacer buildingPlacer)
    {
        _input = input;
        _buildingPlacer = buildingPlacer;

        _input.OnClick += OnClickHandler;
    }

    public void Initialize(Vector2Int position)
    {
        Position = position;
        name = $"Cell_{Position.x}_{Position.y}";
    }

    private void OnClickHandler()
    {
        if (_input.IsOver(this))
        {
            Debug.Log($"Cell {Position} clicked via OnMouseDown");
            _buildingPlacer.Build(this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color color = Color.red;
        switch (Status)
        {
            case CellStatus.Empty:
                color = Color.green;
                break;
            case CellStatus.Block:
                color = Color.red;
                break;
            case CellStatus.Resource:
                color = Color.blue;
                break;
        }
        color.a = 0.25f;

        UtilityShapesDrawer.DrawSolidSquare(transform.position + new Vector3(transform.localScale.x / 2, 0, transform.localScale.z / 2), transform.localScale.x * 0.8f, Quaternion.identity, color);
    }
#endif
}
