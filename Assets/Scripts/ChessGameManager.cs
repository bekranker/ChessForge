using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChessGameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private ChessBoard _chessBoard;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private AIManager _aiManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject winLabel;
    [SerializeField] private GameObject loseLabel;
    [SerializeField] private GameObject phaseUI;
    
    [Header("Game States Props")]
    [SerializeField] private float _betTime = 30f;
    [SerializeField] private float _playTime = 60f;
    [SerializeField] private int setupTurnsPerPlayer = 3;
    
    private float _timerCounter;
    private int currentSetupTurn = 0;
    private int totalSetupTurns;
    
    [Header("Game State")]
    public bool gameActive = true;
    public PlayerColors currentPlayer = PlayerColors.White;
    public GamePhases GamePhase = GamePhases.Setup;
    
    [Header("Turn Management")]
    public bool waitingForPlayerInput = false;
    public bool isPlayerVsAI = true;
    public PlayerColors playerColor = PlayerColors.White;
    
    // Events for UI updates
    public System.Action<GamePhases> OnPhaseChanged;
    public System.Action<PlayerColors> OnPlayerChanged;
    public System.Action<float> OnTimerUpdated;

    IEnumerator Start()
    {
        totalSetupTurns = setupTurnsPerPlayer * 2; // Both players
        
        // Find AI Manager if not assigned
        if (_aiManager == null)
        {
            _aiManager = FindFirstObjectByType<AIManager>();
        }
        
        // Initialize UI
        if (winLabel != null) winLabel.SetActive(false);
        if (loseLabel != null) loseLabel.SetActive(false);
        
        yield return StartCoroutine(StartPhase());
        InitTimer();
        StartCoroutine(GameLoop());
    }
    
    private IEnumerator GameLoop()
    {
        while (gameActive)
        {
            UpdateUI();
            yield return StartCoroutine(ProcessCurrentTurn());
            
            if (ShouldAdvancePhase())
            {
                NextPhase();
            }
            else if (GamePhase != GamePhases.Ended)
            {
                NextPlayer();
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void UpdateUI()
    {
        OnPhaseChanged?.Invoke(GamePhase);
        OnPlayerChanged?.Invoke(currentPlayer);
        OnTimerUpdated?.Invoke(_timerCounter);
    }
    
    private IEnumerator ProcessCurrentTurn()
    {
        Debug.Log($"Processing turn: {GamePhase} - {currentPlayer}");
        
        switch (GamePhase)
        {
            case GamePhases.Setup:
                yield return StartCoroutine(ProcessSetupTurn());
                break;
            case GamePhases.Betting:
                yield return StartCoroutine(ProcessBettingTurn());
                break;
            case GamePhases.Playing:
                yield return StartCoroutine(ProcessPlayingTurn());
                break;
            case GamePhases.Ended:
                ProcessEndGame();
                break;
        }
    }
    
    private IEnumerator ProcessSetupTurn()
    {
        if (IsCurrentPlayerAI())
        {
            if (_aiManager != null)
            {
                yield return StartCoroutine(_aiManager.MakeAIMove());
            }
            else
            {
                Debug.LogWarning("AI Manager not found!");
                yield return new WaitForSeconds(1f);
                OnPlayerAction(); // Skip AI turn
            }
        }
        else
        {
            // Wait for player to place a card
            waitingForPlayerInput = true;
            Debug.Log("Waiting for player to place a card...");
            yield return new WaitUntil(() => !waitingForPlayerInput);
        }
        
        currentSetupTurn++;
    }
    
    private IEnumerator ProcessBettingTurn()
    {
        if (IsCurrentPlayerAI())
        {
            if (_aiManager != null)
            {
                yield return StartCoroutine(_aiManager.MakeAIMove());
            }
            else
            {
                yield return new WaitForSeconds(1f);
                OnPlayerAction();
            }
        }
        else
        {
            // Wait for player to place bets or timer runs out
            waitingForPlayerInput = true;
            Debug.Log("Waiting for player to place bets...");
            yield return new WaitUntil(() => !waitingForPlayerInput || _timerCounter <= 0);
            waitingForPlayerInput = false;
        }
    }
    
    private IEnumerator ProcessPlayingTurn()
    {
        if (IsCurrentPlayerAI())
        {
            if (_aiManager != null)
            {
                yield return StartCoroutine(_aiManager.MakeAIMove());
            }
            else
            {
                yield return new WaitForSeconds(1f);
                OnPlayerAction();
            }
        }
        else
        {
            // Wait for player to make a chess move
            waitingForPlayerInput = true;
            Debug.Log("Waiting for player to make a move...");
            yield return new WaitUntil(() => !waitingForPlayerInput || _timerCounter <= 0);
            waitingForPlayerInput = false;
        }
        
        // Check for game end conditions
        if (IsGameOver())
        {
            GamePhase = GamePhases.Ended;
        }
    }
    
    private void ProcessEndGame()
    {
        gameActive = false;
        
        PlayerColors winner = DetermineWinner();
        Debug.Log($"Game Over! Winner: {winner}");
        
        if (winner == playerColor)
        {
            if (winLabel != null) 
            {
                winLabel.SetActive(true);
                Debug.Log("Player wins!");
            }
        }
        else
        {
            if (loseLabel != null) 
            {
                loseLabel.SetActive(true);
                Debug.Log("Player loses!");
            }
        }
    }
    
    private bool ShouldAdvancePhase()
    {
        switch (GamePhase)
        {
            case GamePhases.Setup:
                return currentSetupTurn >= totalSetupTurns;
            case GamePhases.Betting:
                // Advance after both players have had a chance to bet or timer runs out
                return _timerCounter <= 0 || HasBothPlayersFinishedBetting();
            case GamePhases.Playing:
                return false; // Playing continues until game over
            default:
                return false;
        }
    }
    
    private bool HasBothPlayersFinishedBetting()
    {
        // This is a simplified check - in a real game, you'd track if both players confirmed their bets
        return false; // For now, rely on timer
    }
    
    private bool IsCurrentPlayerAI()
    {
        return isPlayerVsAI && currentPlayer != playerColor;
    }
    
    private bool IsGameOver()
    {
        // Check for checkmate, stalemate, or other end conditions
        PlayerColors enemyColor = currentPlayer == PlayerColors.White ? PlayerColors.Black : PlayerColors.White;
        
        // Check if current player's king is captured
        ChessPiece currentKing = _chessBoard.FindKing(currentPlayer);
        if (currentKing == null)
        {
            return true; // King captured = game over
        }
        
        // Check for checkmate (king in check and no valid moves)
        bool inCheck = _chessBoard.IsInCheck(currentPlayer);
        bool hasValidMoves = HasValidMoves(currentPlayer);
        
        return inCheck && !hasValidMoves;
    }
    
    private bool HasValidMoves(PlayerColors color)
    {
        List<ChessPiece> pieces = _chessBoard.GetPiecesOfColor(color);
        foreach (ChessPiece piece in pieces)
        {
            if (piece.GetValidMoves().Count > 0)
                return true;
        }
        return false;
    }
    
    private PlayerColors DetermineWinner()
    {
        ChessPiece whiteKing = _chessBoard.FindKing(PlayerColors.White);
        ChessPiece blackKing = _chessBoard.FindKing(PlayerColors.Black);
        
        if (whiteKing == null)
            return PlayerColors.Black;
        if (blackKing == null)
            return PlayerColors.White;
        
        // If both kings exist, determine by checkmate
        if (_chessBoard.IsInCheck(PlayerColors.White) && !HasValidMoves(PlayerColors.White))
            return PlayerColors.Black;
        if (_chessBoard.IsInCheck(PlayerColors.Black) && !HasValidMoves(PlayerColors.Black))
            return PlayerColors.White;
        
        // Default to opposite of current player if game ended for other reasons
        return currentPlayer == PlayerColors.White ? PlayerColors.Black : PlayerColors.White;
    }
    
    public void OnPlayerAction()
    {
        waitingForPlayerInput = false;
        Debug.Log("Player action completed");
    }
    
    public void NextPhase()
    {
        switch (GamePhase)
        {
            case GamePhases.Setup:
                GamePhase = GamePhases.Betting;
                currentPlayer = PlayerColors.White; // Reset to first player for betting
                Debug.Log("Advancing to Betting Phase");
                break;
            case GamePhases.Betting:
                GamePhase = GamePhases.Playing;
                currentPlayer = PlayerColors.White; // White moves first in chess
                Debug.Log("Advancing to Playing Phase");
                break;
            case GamePhases.Playing:
                GamePhase = GamePhases.Ended;
                Debug.Log("Game Ended");
                break;
            case GamePhases.Ended:
                gameActive = false;
                break;
        }
        
        InitTimer();
        OnPhaseChanged?.Invoke(GamePhase);
    }
    
    public void NextPlayer()
    {
        currentPlayer = currentPlayer == PlayerColors.White ? PlayerColors.Black : PlayerColors.White;
        Debug.Log($"Turn switched to: {currentPlayer}");
        OnPlayerChanged?.Invoke(currentPlayer);
    }
    
    private IEnumerator StartPhase()
    {
        yield return StartCoroutine(_deckManager.InitDeck());
        Debug.Log("Game initialized - Setup Phase started");
    }
    
    private void InitTimer()
    {
        if (GamePhase == GamePhases.Betting)
        {
            _timerCounter = _betTime;
        }
        else if (GamePhase == GamePhases.Playing)
        {
            _timerCounter = _playTime;
        }
        else
        {
            _timerCounter = 0;
        }
    }
    
    void Update()
    {
        if (_timerCounter > 0 && gameActive)
        {
            _timerCounter -= Time.deltaTime;
            OnTimerUpdated?.Invoke(_timerCounter);
        }
    }
    
    // Public methods for UI
    public string GetCurrentPhaseText()
    {
        switch (GamePhase)
        {
            case GamePhases.Setup: return $"Setup Phase - Turn {currentSetupTurn + 1}/{totalSetupTurns}";
            case GamePhases.Betting: return "Betting Phase";
            case GamePhases.Playing: return "Playing Phase";
            case GamePhases.Ended: return "Game Ended";
            default: return "Unknown Phase";
        }
    }
    
    public string GetCurrentPlayerText()
    {
        if (IsCurrentPlayerAI())
            return "AI Turn";
        else
            return "Your Turn";
    }
    
    // Method to restart the game
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}