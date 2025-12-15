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
    public GameObject AttachedObject;

    public void Initialize(Vector2Int position)
    {
        Position = position;
        name = $"Cell_{Position.x}_{Position.y}";
    }

#if UNITY_EDITOR
    public void DrawGizmos()
    {
        Color color = GetStatusColor(Status);
        color.a = 0.25f;

        UtilityShapesDrawer.DrawWireSquare(transform.position + new Vector3(transform.localScale.x / 2, 0, transform.localScale.z / 2),
         transform.localScale.x * 0.8f, Quaternion.identity, color);
    }

    private Color GetStatusColor(CellStatus status)
    {
        return status switch
        {
            CellStatus.Empty => Color.green,
            CellStatus.Block => Color.red,
            CellStatus.Resource => Color.blue,
            _ => Color.black,
        };
    }
#endif
}
