using UnityEngine;
using System.Collections.Generic;

public enum PieceType
{
    Pawn, Rook, Knight, Bishop, Queen, King
}

public abstract class ChessPiece : MonoBehaviour
{
    [Header("Piece Properties")]
    public PieceType pieceType;
    public int playerIndex; // 0 or 1
    public Vector2Int boardPosition;
    public bool hasMoved = false;
    
    [Header("Betting")]
    public int coinsOnPiece = 0;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    
    protected BoardManager boardManager;
    protected GameManager gameManager;
    
    public virtual void Initialize(PieceType type, int player, Vector2Int position, BoardManager board, GameManager game)
    {
        pieceType = type;
        playerIndex = player;
        boardPosition = position;
        boardManager = board;
        gameManager = game;
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasMoved = false;
        coinsOnPiece = 0;
    }
    
    public abstract List<Vector2Int> GetValidMoves();
    
    public virtual bool CanMoveTo(Vector2Int targetPosition)
    {
        if (!IsPositionOnBoard(targetPosition))
            return false;
            
        ChessPiece targetPiece = boardManager.GetPieceAt(targetPosition);
        
        // Cannot move to a square occupied by own piece
        if (targetPiece != null && targetPiece.playerIndex == playerIndex)
            return false;
            
        return GetValidMoves().Contains(targetPosition);
    }
    
    public virtual void MoveTo(Vector2Int newPosition)
    {
        boardPosition = newPosition;
        hasMoved = true;
        transform.position = boardManager.GetWorldPosition(newPosition);
    }
    
    protected bool IsPositionOnBoard(Vector2Int position)
    {
        return boardManager.IsValidPosition(position);
    }
    
    protected bool IsPathClear(Vector2Int start, Vector2Int end)
    {
        Vector2Int direction = new Vector2Int(
            end.x > start.x ? 1 : end.x < start.x ? -1 : 0,
            end.y > start.y ? 1 : end.y < start.y ? -1 : 0
        );
        
        Vector2Int current = start + direction;
        while (current != end)
        {
            if (boardManager.GetPieceAt(current) != null)
                return false;
            current += direction;
        }
        return true;
    }
    
    protected List<Vector2Int> GetLinearMoves(Vector2Int[] directions, bool limitToOneStep = false)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        foreach (Vector2Int direction in directions)
        {
            int maxDistance = limitToOneStep ? 1 : Mathf.Max(boardManager.boardWidth, boardManager.boardHeight);
            
            for (int i = 1; i <= maxDistance; i++)
            {
                Vector2Int newPos = boardPosition + direction * i;
                
                if (!IsPositionOnBoard(newPos))
                    break;
                    
                ChessPiece targetPiece = boardManager.GetPieceAt(newPos);
                if (targetPiece != null)
                {
                    // Can capture enemy piece
                    if (targetPiece.playerIndex != playerIndex)
                        moves.Add(newPos);
                    break;
                }
                
                moves.Add(newPos);
                
                if (limitToOneStep)
                    break;
            }
        }
        
        return moves;
    }
    
    public void SetCoinsOnPiece(int coins)
    {
        coinsOnPiece = coins;
        Debug.Log($"{pieceType} at {boardPosition} has {coins} coins bet on it");
    }
    
    public int GetCoinsOnPiece()
    {
        return coinsOnPiece;
    }
    
    // Visual feedback for piece selection
    public void SetSelected(bool selected)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = selected ? Color.yellow : (playerIndex == 0 ? Color.white : Color.black);
        }
    }
    
    void OnMouseDown()
    {
        if (gameManager.currentPhase == GamePhase.ChessBattle)
        {
            ChessCombat chessCombat = FindObjectOfType<ChessCombat>();
            if (chessCombat != null)
            {
                chessCombat.HandlePieceClick(this);
            }
        }
    }
}