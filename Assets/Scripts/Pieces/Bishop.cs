using UnityEngine;
using System.Collections.Generic;

public class Bishop : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.Bishop;
    }
    
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        
        // Diagonal directions
        Vector2Int[] directions = {
            new Vector2Int(1, 1),   // Up-right
            new Vector2Int(1, -1),  // Down-right
            new Vector2Int(-1, 1),  // Up-left
            new Vector2Int(-1, -1)  // Down-left
        };
        
        foreach (Vector2Int direction in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                Vector2Int targetPosition = boardPosition + direction * i;
                
                if (!chessBoard.IsValidPosition(targetPosition))
                    break;
                
                ChessPiece pieceAtTarget = chessBoard.GetPieceAt(targetPosition);
                
                if (pieceAtTarget == null)
                {
                    validMoves.Add(targetPosition);
                }
                else
                {
                    if (pieceAtTarget.pieceColor != pieceColor)
                    {
                        validMoves.Add(targetPosition);
                    }
                    break;
                }
            }
        }
        
        return validMoves;
    }
}