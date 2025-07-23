using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float aiThinkingTime = 2f;
    [SerializeField] private int searchDepth = 3;
    [SerializeField] private PlayerColors aiColor = PlayerColors.Black;
    
    private ChessBoard chessBoard;
    private DeckManager deckManager;
    private ChessGameManager gameManager;
    
    [Header("AI Deck")]
    [SerializeField] private List<PieceCard> aiHand = new List<PieceCard>();
    [SerializeField] private List<PieceCardSO> aiDeckData = new List<PieceCardSO>();
    
    public bool IsAITurn => gameManager != null && gameManager.currentPlayer == aiColor;
    
    void Start()
    {
        chessBoard = FindFirstObjectByType<ChessBoard>();
        deckManager = FindFirstObjectByType<DeckManager>();
        gameManager = FindFirstObjectByType<ChessGameManager>();
        
        if (chessBoard == null) Debug.LogError("ChessBoard not found!");
        if (deckManager == null) Debug.LogError("DeckManager not found!");
        if (gameManager == null) Debug.LogError("ChessGameManager not found!");
        
        StartCoroutine(DelayedInitialization());
    }
    
    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(1f);
        InitializeAIDeck();
    }
    
    private void InitializeAIDeck()
    {
        // Load AI deck data from resources or assign manually
        if (aiDeckData.Count == 0)
        {
            PieceCardSO[] allCards = Resources.LoadAll<PieceCardSO>("PieceCards");
            if (allCards.Length > 0)
            {
                foreach (var card in allCards)
                {
                    aiDeckData.Add(card);
                }
            }
        }
        
        // Create AI's hand with random pieces
        int boardSize = PlayerPrefs.GetInt("BoardSize", 8);
        for (int i = 0; i < boardSize && i < aiDeckData.Count; i++)
        {
            CreateAICard(aiDeckData[i % aiDeckData.Count]);
        }
        
        Debug.Log($"AI initialized with {aiHand.Count} cards");
    }
    
    private void CreateAICard(PieceCardSO cardData)
    {
        GameObject aiCardObj = new GameObject($"AI_Card_{cardData.Name}");
        aiCardObj.transform.SetParent(transform);
        
        PieceCard aiCard = aiCardObj.AddComponent<PieceCard>();
        
        // Initialize AI card components
        SpriteRenderer spriteRenderer = aiCardObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardData.IconBlack;
        spriteRenderer.sortingLayerName = "Pieces";
        
        // Initialize the card with data
        aiCard.Initialize(cardData, chessBoard, deckManager);
        aiHand.Add(aiCard);
    }
    
    public IEnumerator MakeAIMove()
    {
        if (!IsAITurn)
        {
            yield break;
        }
        
        yield return new WaitForSeconds(aiThinkingTime);
        
        switch (gameManager.GamePhase)
        {
            case GamePhases.Setup:
                yield return StartCoroutine(AISetupPhase());
                break;
            case GamePhases.Betting:
                yield return StartCoroutine(AIBettingPhase());
                break;
            case GamePhases.Playing:
                yield return StartCoroutine(AIPlayingPhase());
                break;
        }
    }
    
    private IEnumerator AISetupPhase()
    {
        // AI places one card on the board
        List<PieceCard> availableCards = new List<PieceCard>();
        foreach (var card in aiHand)
        {
            if (!card.Putted)
            {
                availableCards.Add(card);
            }
        }
        
        if (availableCards.Count > 0)
        {
            PieceCard cardToPlace = availableCards[Random.Range(0, availableCards.Count)];
            List<TileConfig> availableTiles = chessBoard.GetAvailablePlacementTiles();
            
            if (availableTiles.Count > 0)
            {
                TileConfig targetTile = SelectBestPlacementTile(availableTiles, cardToPlace);
                PlaceAIPiece(cardToPlace, targetTile);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        gameManager.OnPlayerAction();
    }
    
    private TileConfig SelectBestPlacementTile(List<TileConfig> availableTiles, PieceCard card)
    {
        // Simple AI: prefer central positions
        TileConfig bestTile = availableTiles[0];
        float bestScore = EvaluatePosition(bestTile.Position);
        
        foreach (var tile in availableTiles)
        {
            float score = EvaluatePosition(tile.Position);
            if (score > bestScore)
            {
                bestScore = score;
                bestTile = tile;
            }
        }
        
        return bestTile;
    }
    
    private IEnumerator AIBettingPhase()
    {
        // AI makes random bets on placed pieces
        List<ChessPiece> aiPieces = chessBoard.GetPiecesOfColor(aiColor);
        
        foreach (ChessPiece piece in aiPieces)
        {
            if (piece.PieceCard != null)
            {
                float randomBet = Random.Range(25f, 100f);
                // Round to nearest 25
                randomBet = Mathf.Round(randomBet / 25f) * 25f;
                piece.PieceCard.SetBet(randomBet);
            }
        }
        
        yield return new WaitForSeconds(1f);
        gameManager.OnPlayerAction();
    }
    
    private IEnumerator AIPlayingPhase()
    {
        AIMove bestMove = GetBestMove();
        
        if (bestMove != null)
        {
            ChessPiece piece = chessBoard.GetPieceAt(bestMove.from);
            if (piece != null)
            {
                bool moveSuccessful = piece.MoveTo(bestMove.to);
                if (moveSuccessful)
                {
                    Debug.Log($"AI moved {piece.pieceType} from {bestMove.from} to {bestMove.to}");
                }
            }
        }
        
        yield return new WaitForSeconds(1f);
        gameManager.OnPlayerAction();
    }
    
    private AIMove GetBestMove()
    {
        List<AIMove> possibleMoves = GenerateAllPossibleMoves(aiColor);
        
        if (possibleMoves.Count == 0) return null;
        
        AIMove bestMove = possibleMoves[0];
        float bestScore = float.MinValue;
        
        foreach (AIMove move in possibleMoves)
        {
            float score = EvaluateMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    private List<AIMove> GenerateAllPossibleMoves(PlayerColors color)
    {
        List<AIMove> moves = new List<AIMove>();
        List<ChessPiece> pieces = chessBoard.GetPiecesOfColor(color);
        
        foreach (ChessPiece piece in pieces)
        {
            List<Vector2Int> validMoves = piece.GetValidMoves();
            foreach (Vector2Int movePos in validMoves)
            {
                moves.Add(new AIMove(piece.boardPosition, movePos));
            }
        }
        
        return moves;
    }
    
    private float EvaluateMove(AIMove move)
    {
        float score = 0f;
        
        // Basic evaluation: prioritize captures
        ChessPiece targetPiece = chessBoard.GetPieceAt(move.to);
        if (targetPiece != null && targetPiece.pieceColor != aiColor)
        {
            score += GetPieceValue(targetPiece.pieceType);
        }
        
        // Add positional scoring
        score += EvaluatePosition(move.to);
        
        // Avoid moves that put our king in check
        if (WouldMoveResultInCheck(move))
        {
            score -= 1000f;
        }
        
        return score;
    }
    
    private float GetPieceValue(ChessPiece.PieceType type)
    {
        switch (type)
        {
            case ChessPiece.PieceType.Pawn: return 1f;
            case ChessPiece.PieceType.Rook: return 5f;
            case ChessPiece.PieceType.Knight: return 3f;
            case ChessPiece.PieceType.Bishop: return 3f;
            case ChessPiece.PieceType.Queen: return 9f;
            case ChessPiece.PieceType.King: return 100f;
            default: return 0f;
        }
    }
    
    private float EvaluatePosition(Vector2Int position)
    {
        // Simple center control evaluation
        int boardSize = PlayerPrefs.GetInt("BoardSize", 8);
        Vector2 center = new Vector2((boardSize - 1) * 0.5f, (boardSize - 1) * 0.5f);
        float distanceFromCenter = Vector2.Distance(position, center);
        return Mathf.Max(0, (boardSize * 0.5f) - distanceFromCenter);
    }
    
    private bool WouldMoveResultInCheck(AIMove move)
    {
        // Simulate the move
        ChessPiece piece = chessBoard.GetPieceAt(move.from);
        ChessPiece capturedPiece = chessBoard.GetPieceAt(move.to);
        
        // Temporarily make the move
        chessBoard.UpdatePiecePosition(piece, move.from, move.to);
        piece.boardPosition = move.to;
        
        // Check if king is in check
        bool inCheck = chessBoard.IsInCheck(aiColor);
        
        // Restore the board state
        chessBoard.UpdatePiecePosition(piece, move.to, move.from);
        piece.boardPosition = move.from;
        if (capturedPiece != null)
        {
            chessBoard.UpdatePiecePosition(capturedPiece, Vector2Int.zero, move.to);
        }
        
        return inCheck;
    }
    
    private void PlaceAIPiece(PieceCard card, TileConfig tile)
    {
        if (card.Data != null && !tile.Occupied)
        {
            Vector3 worldPos = chessBoard.BoardToWorldPosition(tile.Position);
            ChessPiece piece = Instantiate(card.Data.PiecePrefab, worldPos, Quaternion.identity);
            piece.InitializePiece(card, tile.Position, aiColor);
            tile.SetTile(piece);
            card.Putted = true;
            
            Debug.Log($"AI placed {card.Data.Name} at {tile.Position}");
        }
    }
}

[System.Serializable]
public class AIMove
{
    public Vector2Int from;
    public Vector2Int to;
    
    public AIMove(Vector2Int from, Vector2Int to)
    {
        this.from = from;
        this.to = to;
    }
    
    public override string ToString()
    {
        return $"From {from} to {to}";
    }
}