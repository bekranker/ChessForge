using UnityEngine;
using System.Collections.Generic;

public class King : ChessPiece
{
    public override List<Vector2Int> GetValidMoves()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        return GetLinearMoves(directions, limitToOneStep: true);
    }
    
    public bool IsInCheck()
    {
        ChessCombat chessCombat = FindObjectOfType<ChessCombat>();
        if (chessCombat != null)
        {
            return chessCombat.IsPositionUnderAttack(boardPosition, playerIndex);
        }
        return false;
    }
}