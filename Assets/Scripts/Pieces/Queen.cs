using UnityEngine;
using System.Collections.Generic;

public class Queen : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.Queen;
    }
    
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        
        // Queen combines rook and bishop movements
        Vector2Int[] directions = {
            Vector2Int.up,           // Rook moves
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1),    // Bishop moves
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
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