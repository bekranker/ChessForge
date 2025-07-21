using UnityEngine;
using System.Collections.Generic;

public class King : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        pieceType = PieceType.King;
    }
    
    public override List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        
        // King moves one square in any direction
        Vector2Int[] kingMoves = {
            new Vector2Int(0, 1),    // Up
            new Vector2Int(0, -1),   // Down
            new Vector2Int(1, 0),    // Right
            new Vector2Int(-1, 0),   // Left
            new Vector2Int(1, 1),    // Up-right
            new Vector2Int(1, -1),   // Down-right
            new Vector2Int(-1, 1),   // Up-left
            new Vector2Int(-1, -1)   // Down-left
        };
        
        foreach (Vector2Int move in kingMoves)
        {
            Vector2Int targetPosition = boardPosition + move;
            
            if (chessBoard.IsValidPosition(targetPosition))
            {
                ChessPiece pieceAtTarget = chessBoard.GetPieceAt(targetPosition);
                
                if (pieceAtTarget == null || pieceAtTarget.pieceColor != pieceColor)
                {
                    // Check if the target square is safe (not under attack)
                    if (!IsSquareUnderAttack(targetPosition))
                    {
                        validMoves.Add(targetPosition);
                    }
                }
            }
        }
        
        // Add castling moves
        AddCastlingMoves(validMoves);
        
        return validMoves;
    }
    
    private bool IsSquareUnderAttack(Vector2Int square)
    {
        // Check if any enemy piece can attack this square
        List<ChessPiece> enemyPieces = chessBoard.GetPiecesOfColor(pieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White);
        
        foreach (ChessPiece enemyPiece in enemyPieces)
        {
            if (enemyPiece.IsAttackingSquare(square))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void AddCastlingMoves(List<Vector2Int> validMoves)
    {
        if (hasMoved || IsSquareUnderAttack(boardPosition))
            return;
        
        // Kingside castling
        if (CanCastleKingside())
        {
            validMoves.Add(boardPosition + new Vector2Int(2, 0));
        }
        
        // Queenside castling
        if (CanCastleQueenside())
        {
            validMoves.Add(boardPosition + new Vector2Int(-2, 0));
        }
    }
    
    private bool CanCastleKingside()
    {
        // Check for rook at kingside
        Vector2Int rookPosition = new Vector2Int(7, boardPosition.y);
        ChessPiece rook = chessBoard.GetPieceAt(rookPosition);
        
        if (rook == null || rook.pieceType != PieceType.Rook || rook.hasMoved)
            return false;
        
        // Check if path is clear
        for (int x = boardPosition.x + 1; x < rookPosition.x; x++)
        {
            Vector2Int checkPosition = new Vector2Int(x, boardPosition.y);
            if (chessBoard.GetPieceAt(checkPosition) != null || IsSquareUnderAttack(checkPosition))
                return false;
        }
        
        // Check if final king position is safe
        Vector2Int finalKingPosition = boardPosition + new Vector2Int(2, 0);
        return !IsSquareUnderAttack(finalKingPosition);
    }
    
    private bool CanCastleQueenside()
    {
        // Check for rook at queenside
        Vector2Int rookPosition = new Vector2Int(0, boardPosition.y);
        ChessPiece rook = chessBoard.GetPieceAt(rookPosition);
        
        if (rook == null || rook.pieceType != PieceType.Rook || rook.hasMoved)
            return false;
        
        // Check if path is clear
        for (int x = boardPosition.x - 1; x > rookPosition.x; x--)
        {
            Vector2Int checkPosition = new Vector2Int(x, boardPosition.y);
            if (chessBoard.GetPieceAt(checkPosition) != null || IsSquareUnderAttack(checkPosition))
                return false;
        }
        
        // Check if final king position is safe
        Vector2Int finalKingPosition = boardPosition + new Vector2Int(-2, 0);
        return !IsSquareUnderAttack(finalKingPosition);
    }
    
    public override bool MoveTo(Vector2Int targetPosition)
    {
        Vector2Int oldPosition = boardPosition;
        bool moved = base.MoveTo(targetPosition);
        
        if (moved)
        {
            // Handle castling
            int deltaX = targetPosition.x - oldPosition.x;
            if (Mathf.Abs(deltaX) == 2)
            {
                PerformCastling(deltaX > 0);
            }
        }
        
        return moved;
    }
    
    private void PerformCastling(bool kingside)
    {
        Vector2Int rookOldPosition, rookNewPosition;
        
        if (kingside)
        {
            rookOldPosition = new Vector2Int(7, boardPosition.y);
            rookNewPosition = new Vector2Int(5, boardPosition.y);
        }
        else
        {
            rookOldPosition = new Vector2Int(0, boardPosition.y);
            rookNewPosition = new Vector2Int(3, boardPosition.y);
        }
        
        ChessPiece rook = chessBoard.GetPieceAt(rookOldPosition);
        if (rook != null)
        {
            rook.boardPosition = rookNewPosition;
            rook.hasMoved = true;
            chessBoard.UpdatePiecePosition(rook, rookOldPosition, rookNewPosition);
            
            Vector3 rookWorldPosition = chessBoard.BoardToWorldPosition(rookNewPosition);
            rook.transform.position = rookWorldPosition;
        }
    }
}