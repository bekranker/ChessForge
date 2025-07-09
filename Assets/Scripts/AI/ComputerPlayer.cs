using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AIPersonality
{
    Aggressive,    // Prefers capturing and attacking
    Defensive,     // Prefers protecting pieces and strong positions
    Balanced,      // Mix of aggressive and defensive
    Random         // Makes random valid moves (easiest difficulty)
}

public enum AIDifficulty
{
    Easy,          // Random moves with some basic logic
    Medium,        // Evaluates positions with simple strategy
    Hard           // Advanced evaluation with deeper thinking
}

[System.Serializable]
public class AISettings
{
    public AIPersonality personality = AIPersonality.Balanced;
    public AIDifficulty difficulty = AIDifficulty.Medium;
    public float thinkingTime = 2f;        // Time AI takes to "think"
    public bool showThinking = true;       // Show AI thinking process in logs
    public float aggressiveness = 0.5f;    // 0 = very defensive, 1 = very aggressive
}

public class ComputerPlayer : MonoBehaviour
{
    [Header("AI Configuration")]
    public AISettings aiSettings = new AISettings();
    
    [Header("AI Player Info")]
    public string computerName = "Computer";
    public int computerPlayerIndex = 1; // Computer is player 1 (Player 2)
    
    private GameManager gameManager;
    private BoardManager boardManager;
    private CardSystem cardSystem;
    private ChessCombat chessCombat;
    private BettingSystem bettingSystem;
    
    private bool isThinking = false;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        boardManager = manager.boardManager;
        cardSystem = manager.cardSystem;
        chessCombat = manager.chessCombat;
        bettingSystem = manager.bettingSystem;
        
        Debug.Log($"Computer Player initialized: {computerName} (Difficulty: {aiSettings.difficulty}, Personality: {aiSettings.personality})");
    }
    
    public bool IsComputerTurn()
    {
        return gameManager.currentPlayer == computerPlayerIndex && !isThinking;
    }
    
    public void HandleAITurn()
    {
        if (isThinking || gameManager.currentPlayer != computerPlayerIndex)
            return;
            
        switch (gameManager.currentPhase)
        {
            case GamePhase.CardDeployment:
                StartCoroutine(HandleCardDeploymentAI());
                break;
            case GamePhase.BettingPhase:
                StartCoroutine(HandleBettingAI());
                break;
            case GamePhase.ChessBattle:
                StartCoroutine(HandleChessBattleAI());
                break;
        }
    }
    
    IEnumerator HandleCardDeploymentAI()
    {
        isThinking = true;
        
        if (aiSettings.showThinking)
            Debug.Log($"{computerName} is thinking about card placement...");
        
        yield return new WaitForSeconds(aiSettings.thinkingTime);
        
        // Get available cards for computer
        List<Card> computerHand = cardSystem.GetPlayerHand(computerPlayerIndex);
        
        if (computerHand.Count == 0)
        {
            // Try to draw a card from deck
            if (!cardSystem.TakeACardFromDeck(computerPlayerIndex))
            {
                Debug.Log($"{computerName} has no cards and cannot draw more!");
                gameManager.NextTurn();
                isThinking = false;
                yield break;
            }
            
            computerHand = cardSystem.GetPlayerHand(computerPlayerIndex);
        }
        
        // Select best card to play
        Card selectedCard = SelectBestCard(computerHand);
        
        if (selectedCard != null)
        {
            cardSystem.SelectCard(selectedCard);
            
            // Find best position to place the card
            Vector2Int bestPosition = FindBestCardPlacement(selectedCard);
            
            if (bestPosition != Vector2Int.one * -1) // Valid position found
            {
                if (aiSettings.showThinking)
                    Debug.Log($"{computerName} places {selectedCard.pieceType} at {bestPosition}");
                
                cardSystem.TryPlaceCardAt(bestPosition);
            }
            else
            {
                Debug.LogWarning($"{computerName} couldn't find a valid position for {selectedCard.pieceType}!");
                gameManager.NextTurn();
            }
        }
        else
        {
            Debug.LogWarning($"{computerName} has no playable cards!");
            gameManager.NextTurn();
        }
        
        isThinking = false;
    }
    
    IEnumerator HandleBettingAI()
    {
        isThinking = true;
        
        if (aiSettings.showThinking)
            Debug.Log($"{computerName} is thinking about betting strategy...");
        
        yield return new WaitForSeconds(aiSettings.thinkingTime * 0.5f);
        
        // Simple AI betting logic
        List<ChessPiece> computerPieces = boardManager.GetAllPiecesForPlayer(computerPlayerIndex);
        
        if (computerPieces.Count > 0)
        {
            // Get remaining coins for computer
            int remainingCoins = bettingSystem.GetRemainingCoins(computerPlayerIndex);
            
            // Distribute coins based on piece value and position
            foreach (ChessPiece piece in computerPieces)
            {
                int betAmount = CalculateBetAmount(piece);
                betAmount = Mathf.Min(betAmount, remainingCoins); // Don't exceed remaining coins
                
                if (betAmount > 0)
                {
                    if (aiSettings.showThinking)
                        Debug.Log($"{computerName} bets {betAmount} coins on {piece.pieceType} at {piece.boardPosition}");
                    
                    bettingSystem.PlaceBetOnPiece(piece, betAmount);
                    remainingCoins -= betAmount;
                    
                    if (remainingCoins <= 0) break; // No more coins to bet
                }
            }
        }
        
        // Mark betting as completed for computer
        if (aiSettings.showThinking)
            Debug.Log($"{computerName} completed betting");
            
        bettingSystem.CompleteBettingForPlayer(computerPlayerIndex);
        
        // Check if both players completed, if so transition to battle phase
        if (bettingSystem.BothPlayersCompletedBetting())
        {
            gameManager.StartChessBattlePhase();
        }
        else
        {
            gameManager.NextTurn();
        }
        
        isThinking = false;
    }
    
    IEnumerator HandleChessBattleAI()
    {
        isThinking = true;
        
        if (aiSettings.showThinking)
            Debug.Log($"{computerName} is analyzing the board...");
        
        yield return new WaitForSeconds(aiSettings.thinkingTime);
        
        // Find best move for computer
        AIMove bestMove = FindBestMove();
        
        if (bestMove != null && bestMove.IsValid())
        {
            if (aiSettings.showThinking)
            {
                string moveDescription = bestMove.isCapture ? 
                    $"captures at {bestMove.to}" : 
                    $"moves to {bestMove.to}";
                Debug.Log($"{computerName}: {bestMove.piece.pieceType} {moveDescription} (Score: {bestMove.score:F2})");
            }
            
            // Execute the move
            ExecuteAIMove(bestMove);
        }
        else
        {
            Debug.LogWarning($"{computerName} has no valid moves!");
            gameManager.NextTurn();
        }
        
        isThinking = false;
    }
    
    Card SelectBestCard(List<Card> hand)
    {
        if (hand.Count == 0) return null;
        
        switch (aiSettings.difficulty)
        {
            case AIDifficulty.Easy:
                // Random selection
                return hand[Random.Range(0, hand.Count)];
                
            case AIDifficulty.Medium:
                // Prefer higher value pieces
                return hand.OrderByDescending(card => GetPieceValue(card.pieceType)).First();
                
            case AIDifficulty.Hard:
                // Consider board state and strategy
                return SelectCardStrategically(hand);
                
            default:
                return hand[0];
        }
    }
    
    Card SelectCardStrategically(List<Card> hand)
    {
        Dictionary<Card, float> cardScores = new Dictionary<Card, float>();
        
        foreach (Card card in hand)
        {
            float score = 0f;
            
            // Base piece value
            score += GetPieceValue(card.pieceType);
            
            // Consider deployment zone control
            List<Vector2Int> validPositions = GetValidCardPositions(card);
            score += validPositions.Count * 0.5f;
            
            // Personality-based adjustments
            switch (aiSettings.personality)
            {
                case AIPersonality.Aggressive:
                    if (card.pieceType == PieceType.Queen || card.pieceType == PieceType.Rook)
                        score += 2f;
                    break;
                case AIPersonality.Defensive:
                    if (card.pieceType == PieceType.Pawn || card.pieceType == PieceType.Bishop)
                        score += 1f;
                    break;
            }
            
            cardScores[card] = score;
        }
        
        return cardScores.OrderByDescending(kvp => kvp.Value).First().Key;
    }
    
    Vector2Int FindBestCardPlacement(Card card)
    {
        List<Vector2Int> validPositions = GetValidCardPositions(card);
        
        if (validPositions.Count == 0)
            return Vector2Int.one * -1; // Invalid position indicator
            
        switch (aiSettings.difficulty)
        {
            case AIDifficulty.Easy:
                return validPositions[Random.Range(0, validPositions.Count)];
                
            case AIDifficulty.Medium:
            case AIDifficulty.Hard:
                return EvaluateBestPosition(card, validPositions);
                
            default:
                return validPositions[0];
        }
    }
    
    Vector2Int EvaluateBestPosition(Card card, List<Vector2Int> positions)
    {
        Vector2Int bestPosition = positions[0];
        float bestScore = -1000f;
        
        foreach (Vector2Int pos in positions)
        {
            float score = EvaluatePosition(card, pos);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = pos;
            }
        }
        
        return bestPosition;
    }
    
    float EvaluatePosition(Card card, Vector2Int position)
    {
        float score = 0f;
        
        // Prefer center positions
        int centerX = boardManager.boardWidth / 2;
        int centerY = boardManager.boardHeight / 2;
        float distanceFromCenter = Vector2Int.Distance(position, new Vector2Int(centerX, centerY));
        score += (5f - distanceFromCenter) * 0.5f;
        
        // Consider piece type specific positioning
        switch (card.pieceType)
        {
            case PieceType.Pawn:
                // Pawns prefer front lines
                if (boardManager.IsPlayer2DeploymentZone(position.x, position.y))
                    score += 2f;
                break;
                
            case PieceType.Rook:
                // Rooks prefer corners and edges
                if (position.x == 0 || position.x == boardManager.boardWidth - 1)
                    score += 1.5f;
                break;
                
            case PieceType.Queen:
                // Queens prefer central positions
                score += (5f - distanceFromCenter);
                break;
        }
        
        return score;
    }
    
    List<Vector2Int> GetValidCardPositions(Card card)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();
        
        for (int x = 0; x < boardManager.boardWidth; x++)
        {
            for (int y = 0; y < boardManager.boardHeight; y++)
            {
                if (boardManager.CanPlacePieceAt(x, y, computerPlayerIndex))
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return validPositions;
    }
    
    AIMove FindBestMove()
    {
        List<ChessPiece> computerPieces = boardManager.GetAllPiecesForPlayer(computerPlayerIndex);
        List<AIMove> allMoves = new List<AIMove>();
        
        // Generate all possible moves
        foreach (ChessPiece piece in computerPieces)
        {
            List<Vector2Int> validMoves = piece.GetValidMoves();
            
            foreach (Vector2Int move in validMoves)
            {
                AIMove aiMove = new AIMove
                {
                    piece = piece,
                    from = piece.boardPosition,
                    to = move,
                    isCapture = boardManager.GetPieceAt(move) != null
                };
                
                if (aiMove.isCapture)
                {
                    aiMove.capturedPiece = boardManager.GetPieceAt(move);
                }
                
                aiMove.score = EvaluateMove(aiMove);
                allMoves.Add(aiMove);
            }
        }
        
        if (allMoves.Count == 0)
            return null;
            
        // Select move based on difficulty
        switch (aiSettings.difficulty)
        {
            case AIDifficulty.Easy:
                return allMoves[Random.Range(0, allMoves.Count)];
                
            case AIDifficulty.Medium:
                // Pick from top 3 moves
                allMoves = allMoves.OrderByDescending(m => m.score).Take(3).ToList();
                return allMoves[Random.Range(0, allMoves.Count)];
                
            case AIDifficulty.Hard:
                return allMoves.OrderByDescending(m => m.score).First();
                
            default:
                return allMoves[0];
        }
    }
    
    float EvaluateMove(AIMove move)
    {
        float score = 0f;
        
        // Capture value
        if (move.isCapture)
        {
            score += GetPieceValue(move.capturedPiece.pieceType) * 10f;
        }
        
        // Position improvement
        score += EvaluatePositionalValue(move.piece, move.to);
        
        // Safety (avoid moving into danger)
        if (chessCombat.IsPositionUnderAttack(move.to, computerPlayerIndex))
        {
            score -= GetPieceValue(move.piece.pieceType) * 5f;
        }
        
        // Personality adjustments
        switch (aiSettings.personality)
        {
            case AIPersonality.Aggressive:
                if (move.isCapture) score += 5f;
                break;
            case AIPersonality.Defensive:
                if (!move.isCapture) score += 2f;
                break;
        }
        
        // Add some randomness to make AI less predictable
        score += Random.Range(-1f, 1f);
        
        return score;
    }
    
    float EvaluatePositionalValue(ChessPiece piece, Vector2Int position)
    {
        float value = 0f;
        
        // Center control
        int centerX = boardManager.boardWidth / 2;
        int centerY = boardManager.boardHeight / 2;
        float distanceFromCenter = Vector2Int.Distance(position, new Vector2Int(centerX, centerY));
        value += (5f - distanceFromCenter) * 0.3f;
        
        // Piece-specific positioning
        switch (piece.pieceType)
        {
            case PieceType.Pawn:
                // Pawns want to advance
                if (position.y < piece.boardPosition.y)
                    value += 1f;
                break;
                
            case PieceType.Knight:
                // Knights prefer central positions
                value += (3f - distanceFromCenter) * 0.5f;
                break;
        }
        
        return value;
    }
    
    void ExecuteAIMove(AIMove move)
    {
        // Simulate piece selection and movement
        chessCombat.HandlePieceClick(move.piece);
        chessCombat.HandleTileClick(move.to);
    }
    
    int CalculateBetAmount(ChessPiece piece)
    {
        int baseAmount = GetPieceValue(piece.pieceType);
        
        // Adjust based on position
        if (IsGoodPosition(piece))
            baseAmount += 2;
            
        // Adjust based on personality
        switch (aiSettings.personality)
        {
            case AIPersonality.Aggressive:
                if (piece.pieceType == PieceType.Queen || piece.pieceType == PieceType.Rook)
                    baseAmount += 3;
                break;
            case AIPersonality.Defensive:
                baseAmount = Mathf.Max(1, baseAmount - 1);
                break;
        }
        
        return Mathf.Clamp(baseAmount, 1, 10);
    }
    
    bool IsGoodPosition(ChessPiece piece)
    {
        // Simple position evaluation
        int centerX = boardManager.boardWidth / 2;
        int centerY = boardManager.boardHeight / 2;
        float distanceFromCenter = Vector2Int.Distance(piece.boardPosition, new Vector2Int(centerX, centerY));
        
        return distanceFromCenter <= 2f;
    }
    
    int GetPieceValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Pawn: return 1;
            case PieceType.Bishop:
            case PieceType.Knight: return 3;
            case PieceType.Rook: return 5;
            case PieceType.Queen: return 9;
            case PieceType.King: return 100;
            default: return 1;
        }
    }
    
    public void SetDifficulty(AIDifficulty difficulty)
    {
        aiSettings.difficulty = difficulty;
        Debug.Log($"{computerName} difficulty set to: {difficulty}");
    }
    
    public void SetPersonality(AIPersonality personality)
    {
        aiSettings.personality = personality;
        Debug.Log($"{computerName} personality set to: {personality}");
    }
}

[System.Serializable]
public class AIMove
{
    public ChessPiece piece;
    public Vector2Int from;
    public Vector2Int to;
    public bool isCapture;
    public ChessPiece capturedPiece;
    public float score;
    
    public bool IsValid()
    {
        return piece != null && from != to;
    }
}