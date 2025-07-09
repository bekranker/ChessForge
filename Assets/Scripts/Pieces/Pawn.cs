using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // Pawn direction depends on player (Player 0 moves up, Player 1 moves down)
        int direction = playerIndex == 0 ? 1 : -1;
        
        // Forward movement
        Vector2Int oneStep = boardPosition + new Vector2Int(0, direction);
        if (IsPositionOnBoard(oneStep) && boardManager.GetPieceAt(oneStep) == null)
        {
            moves.Add(oneStep);
            
            // Two-step move from starting position
            if (!hasMoved)
            {
                Vector2Int twoStep = boardPosition + new Vector2Int(0, direction * 2);
                if (IsPositionOnBoard(twoStep) && boardManager.GetPieceAt(twoStep) == null)
                {
                    moves.Add(twoStep);
                }
            }
        }
        
        // Diagonal captures
        Vector2Int[] captureDirections = {
            new Vector2Int(-1, direction),
            new Vector2Int(1, direction)
        };
        
        foreach (Vector2Int captureDir in captureDirections)
        {
            Vector2Int capturePos = boardPosition + captureDir;
            if (IsPositionOnBoard(capturePos))
            {
                ChessPiece targetPiece = boardManager.GetPieceAt(capturePos);
                if (targetPiece != null && targetPiece.playerIndex != playerIndex)
                {
                    moves.Add(capturePos);
                }
            }
        }
        
        return moves;
    }
}