using UnityEngine;
using System.Collections.Generic;

public enum GamePhase
{
    Setup,
    CardDeployment,
    BettingPhase,
    ChessBattle,
    GameEnd
}

public enum BoardSize
{
    Size3x3 = 3,
    Size4x4 = 4,
    Size5x5 = 5,
    Size6x6 = 6,
    Size7x7 = 7,
    Size8x8 = 8
}

public enum GameMode
{
    PlayerVsComputer  // Only support Player vs Computer mode
}

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GamePhase currentPhase = GamePhase.Setup;
    public BoardSize selectedBoardSize = BoardSize.Size8x8;
    public GameMode gameMode = GameMode.PlayerVsComputer; // Always Player vs Computer
    public int currentPlayer = 0; // 0 = Human Player, 1 = Computer
    public int currentTurn = 0;
    public float turnTimeLimit = 30f;
    public float currentTurnTime = 0f;

    [Header("Game Configuration")]
    public GameConfig gameConfig;

    [Header("Systems")]
    public BoardManager boardManager;
    public CardSystem cardSystem;
    public BettingSystem bettingSystem;
    public ChessCombat chessCombat;
    public SimpleUIManager uiManager;
    public PlayerManager playerManager;
    public ComputerPlayer computerPlayer;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI phaseText;
    public TMPro.TextMeshProUGUI playerText;
    public TMPro.TextMeshProUGUI turnText;
    public TMPro.TextMeshProUGUI timerText;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        UpdateTurnTimer();
        UpdateUI();

        // Handle AI turn in Player vs Computer mode
        if (computerPlayer != null)
        {
            if (computerPlayer.IsComputerTurn())
            {
                computerPlayer.HandleAITurn();
            }
        }
    }

    void InitializeGame()
    {
        if (gameConfig == null)
        {
            gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.Initialize();
        }

        InitializeSystems();
        StartSetupPhase();
    }

    void InitializeSystems()
    {
        if (boardManager == null) boardManager = FindObjectOfType<BoardManager>();
        if (cardSystem == null) cardSystem = FindObjectOfType<CardSystem>();
        if (bettingSystem == null) bettingSystem = FindObjectOfType<BettingSystem>();
        if (chessCombat == null) chessCombat = FindObjectOfType<ChessCombat>();
        if (uiManager == null) uiManager = FindObjectOfType<SimpleUIManager>();
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        if (computerPlayer == null) computerPlayer = FindObjectOfType<ComputerPlayer>();

        // Initialize each system
        boardManager?.Initialize(this);
        cardSystem?.Initialize(this);
        bettingSystem?.Initialize(this);
        chessCombat?.Initialize(this);
        uiManager?.Initialize(this);
        playerManager?.Initialize(this);
        computerPlayer?.Initialize(this);
    }

    public void StartSetupPhase()
    {
        currentPhase = GamePhase.Setup;
        Debug.Log("Setup Phase: Choose board size and prepare for battle!");
    }

    public void StartCardDeploymentPhase()
    {
        currentPhase = GamePhase.CardDeployment;
        currentPlayer = 0;
        currentTurn = 0;
        currentTurnTime = turnTimeLimit;

        // Setup board with selected size
        boardManager.SetupBoard(selectedBoardSize);

        // Create slots based on board size
        cardSystem.CreateSlotsForBoardSize((int)selectedBoardSize);

        // Initialize card hands for both players
        cardSystem.InitializeCardPhase();

        Debug.Log($"Card Deployment Phase started! Board size: {selectedBoardSize}");
    }

    public void StartBettingPhase()
    {
        currentPhase = GamePhase.BettingPhase;
        currentPlayer = 0;
        currentTurnTime = turnTimeLimit;

        bettingSystem.StartBettingPhase();
        Debug.Log("Betting Phase: Place your coin bets on deployed pieces!");
    }

    public void StartChessBattlePhase()
    {
        currentPhase = GamePhase.ChessBattle;
        currentPlayer = 0;
        currentTurnTime = turnTimeLimit;

        Debug.Log("🚀 Starting Chess Battle Phase - revealing all pieces...");
        
        // Reveal all pieces that were hidden during deployment and betting
        if (cardSystem != null)
        {
            cardSystem.RevealAllPieces();
        }
        else
        {
            Debug.LogError("❌ CardSystem is null! Cannot reveal pieces.");
        }

        if (chessCombat != null)
        {
            chessCombat.StartBattle();
        }
        else
        {
            Debug.LogError("❌ ChessCombat is null! Cannot start battle.");
        }
        
        Debug.Log("⚔️ Chess Battle Phase: Eliminate all enemy pieces!");
    }

    public void EndGame(int winnerPlayer)
    {
        currentPhase = GamePhase.GameEnd;

        // Calculate rewards
        bettingSystem.CalculateGameRewards(winnerPlayer);

        string winnerName = GetPlayerName(winnerPlayer);
        if (winnerPlayer == -1)
        {
            Debug.Log("Game Over! It's a draw!");
        }
        else
        {
            Debug.Log($"Game Over! {winnerName} wins!");
        }
    }

    public void NextTurn()
    {
        switch (currentPhase)
        {
            case GamePhase.CardDeployment:
                HandleCardDeploymentTurn();
                break;
            case GamePhase.BettingPhase:
                HandleBettingTurn();
                break;
            case GamePhase.ChessBattle:
                HandleChessBattleTurn();
                break;
        }

        currentTurnTime = turnTimeLimit;
    }

    void HandleCardDeploymentTurn()
    {
        currentPlayer = (currentPlayer + 1) % 2;

        if (currentPlayer == 0)
        {
            currentTurn++;

            // Check if card deployment phase is complete
            int maxTurns = gameConfig.GetTurnsForBoardSize(selectedBoardSize);
            if (currentTurn >= maxTurns)
            {
                StartBettingPhase();
                return;
            }
        }

        // Draw card for current player
        cardSystem.DrawCardForPlayer(currentPlayer);
    }

    void HandleBettingTurn()
    {
        Debug.Log($"HandleBettingTurn: Current player {currentPlayer + 1}");
        
        // Check if current player has completed betting
        if (bettingSystem.IsPlayerBettingComplete(currentPlayer))
        {
            Debug.Log($"Player {currentPlayer + 1} already completed betting");
        }
        
        // Switch to next player
        currentPlayer = (currentPlayer + 1) % 2;
        Debug.Log($"Switched to player {currentPlayer + 1}");

        // Check if both players have completed betting
        if (bettingSystem.BothPlayersCompletedBetting())
        {
            Debug.Log("Both players completed betting, starting Chess Battle phase");
            StartChessBattlePhase();
            return;
        }
        
        // If current player hasn't completed betting, let them continue
        if (!bettingSystem.IsPlayerBettingComplete(currentPlayer))
        {
            Debug.Log($"Player {currentPlayer + 1} can continue betting");
        }
    }

    void HandleChessBattleTurn()
    {
        currentPlayer = (currentPlayer + 1) % 2;

        // Check win conditions
        int winner = chessCombat.CheckWinCondition();
        if (winner >= 0) // Only end game for valid player indices (0 or 1)
        {
            EndGame(winner);
        }
        else if (winner == -1) // Draw
        {
            EndGame(-1);
        }
        // If winner == -2, game continues (do nothing)
    }

    void UpdateTurnTimer()
    {
        if (currentPhase == GamePhase.Setup || currentPhase == GamePhase.GameEnd)
            return;

        currentTurnTime -= Time.deltaTime;

        if (currentTurnTime <= 0)
        {
            // Time expired - handle based on phase
            HandleTimeExpired();
        }
    }

    void HandleTimeExpired()
    {
        switch (currentPhase)
        {
            case GamePhase.CardDeployment:
                // Skip turn without placing card
                NextTurn();
                break;
            case GamePhase.BettingPhase:
                // Auto-distribute remaining coins evenly
                bettingSystem.AutoDistributeRemainingCoins(currentPlayer);
                NextTurn();
                break;
            case GamePhase.ChessBattle:
                // Current player loses
                EndGame((currentPlayer + 1) % 2);
                break;
        }
    }

    void UpdateUI()
    {
        if (phaseText != null)
            phaseText.text = $"Phase: {currentPhase}";

        if (playerText != null)
        {
            string playerName = GetCurrentPlayerName();
            playerText.text = $"Current Player: {playerName}";
        }

        if (turnText != null)
            turnText.text = $"Turn: {currentTurn + 1}";

        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentTurnTime)}s";
    }

    public GameConfig GetGameConfig()
    {
        return gameConfig;
    }

    public void SetBoardSize(BoardSize size)
    {
        selectedBoardSize = size;
        Debug.Log($"Board size set to: {size}");
    }

    public string GetCurrentPlayerName()
    {
        return GetPlayerName(currentPlayer);
    }

    public string GetPlayerName(int playerIndex)
    {
        if (playerIndex == 0)
            return "Player";
        else if (playerIndex == 1)
            return computerPlayer != null ? computerPlayer.computerName : "Computer";

        return "Unknown";
    }

    public bool IsComputerPlayer(int playerIndex)
    {
        return playerIndex == 1;
    }

    public bool IsHumanPlayer(int playerIndex)
    {
        return playerIndex == 0;
    }

    public void SetGameMode(GameMode mode)
    {
        // Game mode is always PlayerVsComputer now
        gameMode = GameMode.PlayerVsComputer;
        Debug.Log($"Game mode is always Player vs Computer");
    }
}