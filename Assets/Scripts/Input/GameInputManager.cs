using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    [Header("Input References")]
    public KeyCode selectRandomCardKey = KeyCode.R;
    public KeyCode drawCardKey = KeyCode.D;
    public KeyCode completeBettingKey = KeyCode.B;
    public KeyCode showHandKey = KeyCode.H;
    
    private GameManager gameManager;
    private CardSystem cardSystem;
    private BettingSystem bettingSystem;
    
    void Start()
    {
        // Find game systems
        gameManager = FindObjectOfType<GameManager>();
        cardSystem = FindObjectOfType<CardSystem>();
        bettingSystem = FindObjectOfType<BettingSystem>();
    }
    
    void Update()
    {
        HandleKeyboardInput();
    }
    
    void HandleKeyboardInput()
    {
        if (gameManager == null) return;
        
        // Only process input for human player
        if (gameManager.currentPlayer != 0) return;
        
        switch (gameManager.currentPhase)
        {
            case GamePhase.CardDeployment:
                HandleCardDeploymentInput();
                break;
            case GamePhase.BettingPhase:
                HandleBettingInput();
                break;
            case GamePhase.ChessBattle:
                HandleChessBattleInput();
                break;
        }
    }
    
    void HandleCardDeploymentInput()
    {
        // R - Select random card from hand (keeping this as fallback)
        if (Input.GetKeyDown(selectRandomCardKey))
        {
            if (cardSystem != null)
            {
                cardSystem.SelectRandomCardFromCurrentPlayerHand();
            }
        }
        
        // D - Draw card from deck
        if (Input.GetKeyDown(drawCardKey))
        {
            if (cardSystem != null)
            {
                cardSystem.TakeACardFromDeck(0); // Human player is index 0
            }
        }
        
        // H - Show current hand
        if (Input.GetKeyDown(showHandKey))
        {
            if (cardSystem != null)
            {
                string handInfo = cardSystem.GetCurrentPlayerHandString();
                Debug.Log($"📋 {handInfo}");
            }
        }
        
        // Note: Number key selection removed - using drag & drop instead
    }
    
    void HandleBettingInput()
    {
        // B - Complete betting (auto-distribute remaining coins)
        if (Input.GetKeyDown(completeBettingKey))
        {
            if (bettingSystem != null)
            {
                bettingSystem.OnCompleteBettingClicked();
            }
        }
        
        // H - Show remaining coins
        if (Input.GetKeyDown(showHandKey))
        {
            if (bettingSystem != null)
            {
                int remainingCoins = bettingSystem.GetRemainingCoins(0);
                Debug.Log($"💰 You have {remainingCoins} coins remaining to bet");
            }
        }
    }
    
    void HandleChessBattleInput()
    {
        // H - Show current player info
        if (Input.GetKeyDown(showHandKey))
        {
            string currentPlayerName = gameManager.GetCurrentPlayerName();
            Debug.Log($"⚔️ Current turn: {currentPlayerName}");
        }
        
        // ESC - Deselect piece
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChessCombat chessCombat = FindObjectOfType<ChessCombat>();
            if (chessCombat != null)
            {
                chessCombat.DeselectPiece();
            }
        }
    }
    
    void OnGUI()
    {
        // Show input hints in top-left corner
        if (gameManager == null) return;
        
        // Only show hints for human player
        if (gameManager.currentPlayer != 0) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        
        float y = 10;
        float lineHeight = 20;
        
        GUI.Label(new Rect(10, y, 300, lineHeight), $"Phase: {gameManager.currentPhase}", style);
        y += lineHeight;
        
        switch (gameManager.currentPhase)
        {
            case GamePhase.CardDeployment:
                GUI.Label(new Rect(10, y, 300, lineHeight), $"🎴 Card Deployment Controls:", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [R] - Select random card (fallback)", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [D] - Draw card from deck", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [H] - Show hand", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• Drag cards from hand to board to place", style);
                break;
                
            case GamePhase.BettingPhase:
                GUI.Label(new Rect(10, y, 300, lineHeight), $"💰 Betting Controls:", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [B] - Complete betting (auto-bet)", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [H] - Show remaining coins", style);
                break;
                
            case GamePhase.ChessBattle:
                GUI.Label(new Rect(10, y, 300, lineHeight), $"⚔️ Chess Battle Controls:", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• Click pieces to select, click tiles to move", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [ESC] - Deselect piece", style);
                y += lineHeight;
                GUI.Label(new Rect(10, y, 300, lineHeight), $"• [H] - Show current player", style);
                break;
        }
    }
}