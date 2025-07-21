using UnityEngine;
using System.Collections.Generic;

public class Rook : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.Rook;
    }
    
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        
        // Horizontal and vertical directions
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
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