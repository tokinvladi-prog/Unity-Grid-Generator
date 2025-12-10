using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CellStorage : MonoBehaviour
{
    [field: SerializeField] public Vector2Int GridSize { get; set; }
    [field: SerializeField] public float CellSize { get; set; }
    [field: SerializeField] public Vector3 GridStart { get; set; }
    [field: SerializeField] public List<Cell> Cells { get; set; } = new();

    public static CellStorage Instance { get; private set; }

    public CellStatus GetCellStatus(Vector2Int position)
        => Cells.FirstOrDefault(c => c.Position.Equals(position)).Status;

    public void SetCellStatus(Vector2Int position, CellStatus status)
    {
        var cell = Cells.FirstOrDefault(c => c.Position.Equals(position));
        if (cell != null)
        {
            cell.Status = status;
        }
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetCellStatus(Cell cell, CellStatus status)
    {
        if (cell == null) return;

        cell.Status = status;
    }

    public Cell GetCell(Cell cell)
    {
        return null;
    }
    public Cell GetCell(Vector2Int position)
    {
        return null;
    }


    public int GetPathLength(Cell cellStart, Cell cellEnd)
    {
        return 0;
    }

    public int GetPathLenght(Vector2Int positionCellStart, Vector2Int positionCellEnd)
    {
        return 0;
    }

    public void DestroyAllCells()
    {

    }

    public void DestroyCell()
    {

    }

#if UNITY_EDITOR
    [HideInInspector] public bool VisualizeCellsStatus;

    public void OnDrawGizmos()
    {
        if (!VisualizeCellsStatus) return;

        foreach (var cell in Cells)
        {
            cell.DrawGizmos();
        }
    }
#endif
}