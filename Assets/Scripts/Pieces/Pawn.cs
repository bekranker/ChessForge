using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.Pawn;
    }

    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        int direction = pieceColor == PlayerColors.White ? 1 : -1;
        Vector2Int oneStep = boardPosition + new Vector2Int(0, direction);
        Vector2Int twoStep = boardPosition + new Vector2Int(0, direction * 2);

        // Forward movement
        if (chessBoard.IsValidPosition(oneStep) && chessBoard.GetPieceAt(oneStep) == null)
        {
            validMoves.Add(oneStep);

            // Two squares forward if hasn't moved
            if (!hasMoved && chessBoard.IsValidPosition(twoStep) && chessBoard.GetPieceAt(twoStep) == null)
            {
                validMoves.Add(twoStep);
            }
        }

        // Diagonal captures
        Vector2Int leftCapture = boardPosition + new Vector2Int(-1, direction);
        Vector2Int rightCapture = boardPosition + new Vector2Int(1, direction);

        if (chessBoard.IsValidPosition(leftCapture))
        {
            ChessPiece leftPiece = chessBoard.GetPieceAt(leftCapture);
            if (leftPiece != null && leftPiece.pieceColor != pieceColor)
            {
                validMoves.Add(leftCapture);
            }
        }

        if (chessBoard.IsValidPosition(rightCapture))
        {
            ChessPiece rightPiece = chessBoard.GetPieceAt(rightCapture);
            if (rightPiece != null && rightPiece.pieceColor != pieceColor)
            {
                validMoves.Add(rightCapture);
            }
        }

        // En passant (simplified - can be expanded)
        CheckEnPassant(validMoves, direction);

        return validMoves;
    }

    private void CheckEnPassant(List<Vector2Int> validMoves, int direction)
    {
        // Check left en passant
        Vector2Int leftAdjacent = boardPosition + new Vector2Int(-1, 0);
        if (chessBoard.IsValidPosition(leftAdjacent))
        {
            ChessPiece leftPiece = chessBoard.GetPieceAt(leftAdjacent);
            if (leftPiece is Pawn && leftPiece.pieceColor != pieceColor)
            {
                Pawn leftPawn = leftPiece as Pawn;
                if (CanEnPassant(leftPawn))
                {
                    validMoves.Add(boardPosition + new Vector2Int(-1, direction));
                }
            }
        }

        // Check right en passant
        Vector2Int rightAdjacent = boardPosition + new Vector2Int(1, 0);
        if (chessBoard.IsValidPosition(rightAdjacent))
        {
            ChessPiece rightPiece = chessBoard.GetPieceAt(rightAdjacent);
            if (rightPiece is Pawn && rightPiece.pieceColor != pieceColor)
            {
                Pawn rightPawn = rightPiece as Pawn;
                if (CanEnPassant(rightPawn))
                {
                    validMoves.Add(boardPosition + new Vector2Int(1, direction));
                }
            }
        }
    }

    private bool CanEnPassant(Pawn targetPawn)
    {
        // Simplified en passant check - can be expanded with game state tracking
        return targetPawn.hasMoved && Mathf.Abs(targetPawn.boardPosition.y - GetStartingRow()) == 2;
    }

    private int GetStartingRow()
    {
        return pieceColor == PlayerColors.White ? 1 : 6;
    }

    public override bool MoveTo(Vector2Int targetPosition)
    {
        bool moved = base.MoveTo(targetPosition);
        return moved;
    }
}