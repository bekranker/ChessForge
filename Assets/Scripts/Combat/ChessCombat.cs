using UnityEngine;
using System.Collections.Generic;

public class ChessCombat : MonoBehaviour
{
    [Header("Combat State")]
    public ChessPiece selectedPiece;
    public List<Vector2Int> validMoves = new List<Vector2Int>();

    [Header("Game State Tracking")]
    public List<string> boardStates = new List<string>();
    public Dictionary<string, int> stateCount = new Dictionary<string, int>();

    private GameManager gameManager;
    private BoardManager boardManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        boardManager = manager.boardManager;
    }

    public void StartBattle()
    {
        selectedPiece = null;
        validMoves.Clear();
        boardStates.Clear();
        stateCount.Clear();

        // Record initial board state
        RecordBoardState();

        Debug.Log("Chess battle has begun! Eliminate all enemy pieces to win!");
    }

    public void HandlePieceClick(ChessPiece piece)
    {
        // If clicking on current player's piece, select it
        if (piece.playerIndex == gameManager.currentPlayer)
        {
            SelectPiece(piece);
            return;
        }

        // If clicking on opponent's piece and we have a piece selected, try to capture
        if (selectedPiece != null && piece.playerIndex != gameManager.currentPlayer)
        {
            Vector2Int targetPosition = piece.boardPosition;
            if (validMoves.Contains(targetPosition))
            {
                Debug.Log($"Attempting capture: {selectedPiece.pieceType} at {selectedPiece.boardPosition} captures {piece.pieceType} at {targetPosition}");
                MovePiece(selectedPiece.boardPosition, targetPosition);
            }
            else
            {
                Debug.Log($"Cannot capture: {piece.pieceType} at {targetPosition} is not a valid move for selected {selectedPiece.pieceType}");
                DeselectPiece();
            }
        }
    }

    public void HandleTileClick(Vector2Int position)
    {
        if (selectedPiece == null)
        {
            // Try to select a piece at this position
            ChessPiece piece = boardManager.GetPieceAt(position);
            if (piece != null && piece.playerIndex == gameManager.currentPlayer)
            {
                SelectPiece(piece);
            }
            return;
        }

        // Try to move to this position
        if (validMoves.Contains(position))
        {
            MovePiece(selectedPiece.boardPosition, position);
        }
        else
        {
            // Deselect if clicking invalid position
            DeselectPiece();
        }
    }

    void SelectPiece(ChessPiece piece)
    {
        // Deselect previous piece
        if (selectedPiece != null)
        {
            selectedPiece.SetSelected(false);
        }

        selectedPiece = piece;
        validMoves = piece.GetValidMoves();

        // Debug output for move validation
        Debug.Log($"Selected {piece.pieceType} at {piece.boardPosition}. Found {validMoves.Count} valid moves:");
        foreach (Vector2Int move in validMoves)
        {
            ChessPiece targetPiece = boardManager.GetPieceAt(move);
            if (targetPiece != null)
            {
                Debug.Log($"  - {move} (CAPTURE {targetPiece.pieceType} Player {targetPiece.playerIndex + 1})");
            }
            else
            {
                Debug.Log($"  - {move} (empty)");
            }
        }

        // Visual feedback
        selectedPiece.SetSelected(true);
        boardManager.ResetTileColors();
        boardManager.HighlightTile(piece.boardPosition.x, piece.boardPosition.y, boardManager.highlightColor);
        boardManager.HighlightValidMoves(validMoves);
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.SetSelected(false);
        }

        selectedPiece = null;
        validMoves.Clear();
        boardManager.ResetTileColors();
    }

    void MovePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece piece = boardManager.GetPieceAt(from);
        ChessPiece capturedPiece = boardManager.GetPieceAt(to);

        if (piece == null)
            return;

        // Log the move
        string moveLog = $"Player {gameManager.currentPlayer + 1}: {piece.pieceType} {from} -> {to}";
        if (capturedPiece != null)
        {
            moveLog += $" (captured {capturedPiece.pieceType})";
        }
        Debug.Log(moveLog);

        // Execute the move
        boardManager.MovePiece(from, to);

        // Deselect piece
        DeselectPiece();

        // Record new board state
        RecordBoardState();

        // Check for draw conditions
        if (CheckDrawConditions())
        {
            gameManager.EndGame(-1); // Draw
            return;
        }

        // End turn
        gameManager.NextTurn();
    }

    public int CheckWinCondition()
    {
        int player0Pieces = boardManager.CountPiecesForPlayer(0);
        int player1Pieces = boardManager.CountPiecesForPlayer(1);

        if (player0Pieces == 0 && player1Pieces == 0)
        {
            return -1; // Draw - both players eliminated
        }
        else if (player0Pieces == 0)
        {
            return 1; // Player 2 wins
        }
        else if (player1Pieces == 0)
        {
            return 0; // Player 1 wins
        }

        // Check for stalemate (no legal moves)
        if (HasNoLegalMoves(gameManager.currentPlayer))
        {
            return -1; // Draw - stalemate
        }

        return -2; // Game continues
    }

    bool HasNoLegalMoves(int playerIndex)
    {
        List<ChessPiece> playerPieces = boardManager.GetAllPiecesForPlayer(playerIndex);

        foreach (ChessPiece piece in playerPieces)
        {
            List<Vector2Int> moves = piece.GetValidMoves();
            if (moves.Count > 0)
            {
                return false; // Found at least one legal move
            }
        }

        return true; // No legal moves found
    }

    bool CheckDrawConditions()
    {
        // Check for repeated board states (3-fold repetition)
        string currentState = GetBoardStateString();

        if (stateCount.ContainsKey(currentState))
        {
            stateCount[currentState]++;
            if (stateCount[currentState] >= 3)
            {
                Debug.Log("Draw: 3-fold repetition");
                return true;
            }
        }

        return false;
    }

    void RecordBoardState()
    {
        string stateString = GetBoardStateString();
        boardStates.Add(stateString);

        if (stateCount.ContainsKey(stateString))
        {
            stateCount[stateString]++;
        }
        else
        {
            stateCount[stateString] = 1;
        }
    }

    string GetBoardStateString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = 0; y < boardManager.boardHeight; y++)
        {
            for (int x = 0; x < boardManager.boardWidth; x++)
            {
                ChessPiece piece = boardManager.GetPieceAt(x, y);
                if (piece == null)
                {
                    sb.Append(".");
                }
                else
                {
                    string pieceChar = piece.pieceType.ToString().Substring(0, 1);
                    if (piece.playerIndex == 1)
                        pieceChar = pieceChar.ToLower();
                    sb.Append(pieceChar);
                }
            }
            sb.Append("/");
        }

        // Add current player to move
        sb.Append($" {gameManager.currentPlayer}");

        return sb.ToString();
    }

    public bool IsPositionUnderAttack(Vector2Int position, int defendingPlayer)
    {
        int attackingPlayer = 1 - defendingPlayer;
        List<ChessPiece> attackingPieces = boardManager.GetAllPiecesForPlayer(attackingPlayer);

        foreach (ChessPiece piece in attackingPieces)
        {
            List<Vector2Int> moves = piece.GetValidMoves();
            if (moves.Contains(position))
            {
                return true;
            }
        }

        return false;
    }
}