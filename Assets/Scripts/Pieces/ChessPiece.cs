using UnityEngine;
using System.Collections.Generic;

public abstract partial class ChessPiece : MonoBehaviour
{
    [Header("Piece Properties")]
    public PlayerColors pieceColor;
    public PieceType pieceType;
    public Vector2Int boardPosition;
    public bool hasMoved = false;

    [Header("Movement")]
    public float moveSpeed = 5f;

    protected ChessBoard chessBoard;
    protected bool isSelected = false;
    public PieceCard PieceCard; // Reference to the piece card if applicable
    public void InitializePiece(PieceCard referenceCard, Vector2Int initialPosition, PlayerColors color)
    {
        PieceCard = referenceCard;
        boardPosition = initialPosition;
        pieceColor = color;

    }
    private void SetVisuals()
    {

    }
    protected virtual void Start()
    {
        chessBoard = FindFirstObjectByType<ChessBoard>();
        if (chessBoard == null)
        {
            Debug.LogError("ChessBoard not found in scene!");
        }
    }

    public abstract List<Vector2Int> GetValidMoves();

    public virtual bool CanMoveTo(Vector2Int targetPosition)
    {
        List<Vector2Int> validMoves = GetValidMoves();
        return validMoves.Contains(targetPosition);
    }

    public virtual bool MoveTo(Vector2Int targetPosition)
    {
        if (!CanMoveTo(targetPosition))
        {
            return false;
        }

        ChessPiece targetPiece = chessBoard.GetPieceAt(targetPosition);
        if (targetPiece != null && targetPiece.pieceColor != pieceColor)
        {
            Capture(targetPiece);
        }

        Vector2Int oldPosition = boardPosition;
        boardPosition = targetPosition;
        hasMoved = true;

        chessBoard.UpdatePiecePosition(this, oldPosition, targetPosition);

        Vector3 worldPosition = chessBoard.BoardToWorldPosition(targetPosition);
        transform.position = worldPosition;

        return true;
    }

    public virtual void Capture(ChessPiece targetPiece)
    {
        if (targetPiece != null)
        {
            chessBoard.RemovePiece(targetPiece);
            Destroy(targetPiece.gameObject);
        }
    }

    public virtual bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        if (!chessBoard.IsValidPosition(to))
            return false;

        ChessPiece targetPiece = chessBoard.GetPieceAt(to);
        if (targetPiece != null && targetPiece.pieceColor == pieceColor)
            return false;

        return true;
    }

    protected bool IsPathClear(Vector2Int from, Vector2Int to)
    {
        Vector2Int direction = new Vector2Int(
            to.x != from.x ? (to.x > from.x ? 1 : -1) : 0,
            to.y != from.y ? (to.y > from.y ? 1 : -1) : 0
        );

        Vector2Int current = from + direction;

        while (current != to)
        {
            if (chessBoard.GetPieceAt(current) != null)
                return false;
            current += direction;
        }

        return true;
    }

    public virtual void OnPieceSelected()
    {
        isSelected = true;
        ShowValidMoves();
    }

    public virtual void OnPieceDeselected()
    {
        isSelected = false;
        HideValidMoves();
    }

    protected virtual void ShowValidMoves()
    {
        List<Vector2Int> validMoves = GetValidMoves();
        if (chessBoard != null)
        {
            chessBoard.HighlightValidMoves(validMoves);
        }
    }

    protected virtual void HideValidMoves()
    {
        if (chessBoard != null)
        {
            chessBoard.ClearHighlights();
        }
    }

    public virtual bool IsAttackingSquare(Vector2Int square)
    {
        List<Vector2Int> validMoves = GetValidMoves();
        return validMoves.Contains(square);
    }
}