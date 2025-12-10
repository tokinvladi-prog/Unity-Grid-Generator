using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class CellStorage : MonoBehaviour
{
    #region Fields

    [field: SerializeField] public Vector2Int GridSize { get; set; }
    [field: SerializeField] public float CellSize { get; set; }
    [field: SerializeField] public Vector3 GridStart { get; set; }
    [field: SerializeField] public List<Cell> Cells { get; set; } = new();

    public static CellStorage Instance { get; private set; }

    #endregion

    #region Get/Set Cell Logic

    public void SetCellStatus(Vector2Int position, CellStatus status)
    {
        var cell = Cells.FirstOrDefault(c => c.Position.Equals(position));
        if (cell == null) return;

        cell.Status = status;
    }

    public void SetCellStatus(Cell cell, CellStatus status)
    {
        if (cell == null) return;

        cell.Status = status;
    }

    public CellStatus GetCellStatus(Vector2Int position)
        => Cells.FirstOrDefault(c => c.Position.Equals(position)).Status;

    public CellStatus GetCellStatus(Cell cell)
        => Cells.FirstOrDefault(c => c.Equals(cell)).Status;

    public Cell GetCell(Cell cell)
        => Cells.FirstOrDefault(c => c.Equals(cell));

    public Cell GetCell(Vector2Int position)
        => Cells.FirstOrDefault(c => c.Position.Equals(position));

    #endregion

    public int GetPathLength(Cell cellStart, Cell cellEnd)
    {
        return 0;
    }

    public int GetPathLength(Vector2Int positionCellStart, Vector2Int positionCellEnd)
    {
        return 0;
    }

    public void DestroyAllCells()
    {
        Cells.ForEach(c => DestroyImmediate(c.gameObject));
        Cells.Clear();
    }

    private void OnEnable() => Instance = this;

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
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