using UnityEngine;
using System.Collections.Generic;

public class ChessGameManager : MonoBehaviour
{
    [Header("Game References")]
    public ChessBoard chessBoard;
    
    [Header("Game State")]
    public bool gameActive = true;
    public ChessPiece.PieceColor currentPlayer = ChessPiece.PieceColor.White;
    
    private ChessPiece selectedPiece;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (chessBoard == null)
        {
            chessBoard = FindFirstObjectByType<ChessBoard>();
        }
    }
    
    private void Update()
    {
        if (!gameActive) return;
        
        HandleInput();
    }
    
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    
    private void HandleMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            ChessPiece clickedPiece = hit.collider.GetComponent<ChessPiece>();
            
            if (clickedPiece != null)
            {
                HandlePieceClick(clickedPiece);
            }
            else
            {
                HandleBoardClick(hit.point);
            }
        }
    }
    
    private void HandlePieceClick(ChessPiece piece)
    {
        if (selectedPiece == null)
        {
            // Select piece if it belongs to current player
            if (piece.pieceColor == currentPlayer)
            {
                SelectPiece(piece);
            }
        }
        else
        {
            // If clicking on the same piece, deselect
            if (selectedPiece == piece)
            {
                DeselectPiece();
            }
            // If clicking on an enemy piece, try to capture
            else if (piece.pieceColor != currentPlayer)
            {
                TryMovePiece(piece.boardPosition);
            }
            // If clicking on a friendly piece, select it instead
            else
            {
                DeselectPiece();
                SelectPiece(piece);
            }
        }
    }
    
    private void HandleBoardClick(Vector3 worldPosition)
    {
        if (selectedPiece != null)
        {
            Vector2Int boardPosition = chessBoard.WorldToBoardPosition(worldPosition);
            TryMovePiece(boardPosition);
        }
    }
    
    private void SelectPiece(ChessPiece piece)
    {
        selectedPiece = piece;
        piece.OnPieceSelected();
    }
    
    private void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.OnPieceDeselected();
            selectedPiece = null;
        }
    }
    
    private void TryMovePiece(Vector2Int targetPosition)
    {
        if (selectedPiece == null) return;
        
        if (selectedPiece.CanMoveTo(targetPosition))
        {
            Vector2Int oldPosition = selectedPiece.boardPosition;
            
            // Check if move would put own king in check
            if (WouldMoveExposeKing(selectedPiece, targetPosition))
            {
                Debug.Log("Move would expose king to check!");
                return;
            }
            
            bool moveSuccessful = selectedPiece.MoveTo(targetPosition);
            
            if (moveSuccessful)
            {
                DeselectPiece();
                
                // Check for check/checkmate
                ChessPiece.PieceColor opponentColor = currentPlayer == ChessPiece.PieceColor.White ? 
                                                     ChessPiece.PieceColor.Black : ChessPiece.PieceColor.White;
                
                if (chessBoard.IsInCheck(opponentColor))
                {
                    Debug.Log($"{opponentColor} is in check!");
                    
                    if (IsCheckmate(opponentColor))
                    {
                        Debug.Log($"Checkmate! {currentPlayer} wins!");
                        // Note: Game continues as per requirement
                    }
                }
                else if (IsStalemate(opponentColor))
                {
                    Debug.Log("Stalemate!");
                }
                
                SwitchTurn();
            }
        }
        else
        {
            DeselectPiece();
        }
    }
    
    private bool WouldMoveExposeKing(ChessPiece piece, Vector2Int targetPosition)
    {
        // Simulate the move
        Vector2Int originalPosition = piece.boardPosition;
        ChessPiece capturedPiece = chessBoard.GetPieceAt(targetPosition);
        
        // Temporarily make the move
        chessBoard.UpdatePiecePosition(piece, originalPosition, targetPosition);
        piece.boardPosition = targetPosition;
        
        if (capturedPiece != null)
        {
            chessBoard.RemovePiece(capturedPiece);
        }
        
        // Check if king is in check
        bool kingInCheck = chessBoard.IsInCheck(piece.pieceColor);
        
        // Undo the move
        chessBoard.UpdatePiecePosition(piece, targetPosition, originalPosition);
        piece.boardPosition = originalPosition;
        
        if (capturedPiece != null)
        {
            chessBoard.board[targetPosition.x, targetPosition.y] = capturedPiece;
        }
        
        return kingInCheck;
    }
    
    private bool IsCheckmate(ChessPiece.PieceColor color)
    {
        return chessBoard.IsInCheck(color) && !HasValidMoves(color);
    }
    
    private bool IsStalemate(ChessPiece.PieceColor color)
    {
        return !chessBoard.IsInCheck(color) && !HasValidMoves(color);
    }
    
    private bool HasValidMoves(ChessPiece.PieceColor color)
    {
        List<ChessPiece> pieces = chessBoard.GetPiecesOfColor(color);
        
        foreach (ChessPiece piece in pieces)
        {
            List<Vector2Int> validMoves = piece.GetValidMoves();
            
            foreach (Vector2Int move in validMoves)
            {
                if (!WouldMoveExposeKing(piece, move))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void SwitchTurn()
    {
        currentPlayer = currentPlayer == ChessPiece.PieceColor.White ? 
                       ChessPiece.PieceColor.Black : ChessPiece.PieceColor.White;
        
        chessBoard.SwitchCurrentPlayer();
    }
    
    public void ResetGame()
    {
        gameActive = true;
        currentPlayer = ChessPiece.PieceColor.White;
        DeselectPiece();
        
        // Clear the board and reinitialize
        if (chessBoard != null)
        {
            // Destroy all existing pieces
            ChessPiece[] existingPieces = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);
            foreach (ChessPiece piece in existingPieces)
            {
                DestroyImmediate(piece.gameObject);
            }
            
            // Restart the board
            chessBoard.Start();
        }
    }
    
    public void EndGame()
    {
        gameActive = false;
        DeselectPiece();
    }
}