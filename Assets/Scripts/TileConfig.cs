using UnityEngine;

[System.Serializable]
public class TileConfig
{
    public bool IsWhite { get; set; }
    public Vector2Int Position { get; set; }
    public bool Occupied { get; set; }
    public GameObject TileObject { get; set; }
    public ChessPiece CurrentPiece;
    public TileConfig(Vector2Int position, bool occupied, GameObject tileObject, bool isWhite)
    {
        Position = position;
        Occupied = occupied;
        TileObject = tileObject;
        IsWhite = isWhite;
    }
    public void ClearTile()
    {
        Occupied = false;
        if (TileObject != null)
        {
            Object.Destroy(TileObject);
            TileObject = null;
        }
    }
    public void SetTile(ChessPiece piece)
    {
        if (Occupied) return;
        CurrentPiece = piece;
        CurrentPiece.transform.position = TileObject.transform.position;
        Occupied = true;
    }
}
