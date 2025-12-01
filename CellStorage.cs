using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CellStorage", menuName = "Scriptable Objects/Cell/Cell Storage")]
public class CellStorage : ScriptableObject
{
    [field: SerializeField] public Vector2Int GridSize { get; set; }
    [field: SerializeField] public float CellSize { get; set; }
    [field: SerializeField] public Vector3 GridStart { get; set; }
    [field: SerializeField] public List<Cell> Cells { get; } = new();

    public CellStatus GetCellStatus(Vector2Int position)
        => Cells.FirstOrDefault(c => c.Position.Equals(position))?.Status ?? default;

    public void SetCellStatus(Vector2Int position, CellStatus status)
    {
        var cell = Cells.FirstOrDefault(c => c.Position.Equals(position));
        if (cell != null)
        {
            cell.Status = status;
        }
    }
}