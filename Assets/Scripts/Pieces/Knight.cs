using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        Vector2Int[] knightMoves = {
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(1, -2),
            new Vector2Int(-1, 2), new Vector2Int(-1, -2)
        };
        
        foreach (Vector2Int move in knightMoves)
        {
            Vector2Int newPos = boardPosition + move;
            if (IsPositionOnBoard(newPos))
            {
                ChessPiece targetPiece = boardManager.GetPieceAt(newPos);
                if (targetPiece == null || targetPiece.playerIndex != playerIndex)
                {
                    moves.Add(newPos);
                }
            }
        }
        
        return moves;
    }
}