using UnityEngine;
using System.Collections.Generic;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetValidMoves()
    {
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        return GetLinearMoves(directions);
    }
}