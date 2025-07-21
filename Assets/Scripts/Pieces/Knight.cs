using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.Knight;
    }
    
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        
        // Knight moves in L-shape: 2 squares in one direction, 1 in perpendicular
        Vector2Int[] knightMoves = {
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-2, -1),
            new Vector2Int(1, 2),
            new Vector2Int(1, -2),
            new Vector2Int(-1, 2),
            new Vector2Int(-1, -2)
        };
        
        foreach (Vector2Int move in knightMoves)
        {
            Vector2Int targetPosition = boardPosition + move;
            
            if (chessBoard.IsValidPosition(targetPosition))
            {
                ChessPiece pieceAtTarget = chessBoard.GetPieceAt(targetPosition);
                
                if (pieceAtTarget == null || pieceAtTarget.pieceColor != pieceColor)
                {
                    validMoves.Add(targetPosition);
                }
            }
        }
        
        return validMoves;
    }
}