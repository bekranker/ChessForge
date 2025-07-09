using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleUIManager : MonoBehaviour
{
    // No inspector fields needed! Everything is found automatically
    [SerializeField] TextMeshProUGUI phaseText;
    [SerializeField] TextMeshProUGUI playerText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI instructionsText;
    
    private GameManager gameManager;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        
        // Automatically find all UI elements
        FindUIElements();
        
        // Create any missing UI elements
        CreateMissingUIElements();
        
        Debug.Log("SimpleUIManager initialized successfully with automatic UI discovery!");
    }
    
    void FindUIElements()
    {
        // Find existing UI elements created by ChessForgeSetup
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                if (obj.name.Contains("Phase")) phaseText = text;
                else if (obj.name.Contains("Player")) playerText = text;
                else if (obj.name.Contains("Timer")) timerText = text;
                else if (obj.name.Contains("Instructions")) instructionsText = text;
            }
        }
    }
    
    void CreateMissingUIElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found! UI elements cannot be created.");
            return;
        }
        
        // Create instructions text if missing
        if (instructionsText == null)
        {
            GameObject instructionsObject = new GameObject("Instructions Text");
            instructionsObject.transform.SetParent(canvas.transform);
            
            instructionsText = instructionsObject.AddComponent<TextMeshProUGUI>();
            instructionsText.text = "ChessForge: Press T to start Card Deployment!";
            instructionsText.fontSize = 16;
            instructionsText.color = Color.cyan;
            instructionsText.alignment = TMPro.TextAlignmentOptions.Center;
            
            RectTransform rect = instructionsText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -150);
            rect.sizeDelta = new Vector2(600, 40);
        }
        
        Debug.Log($"UI Elements Status - Phase: {phaseText != null}, Player: {playerText != null}, Timer: {timerText != null}, Instructions: {instructionsText != null}");
    }
    
    public void UpdateUI()
    {
        if (gameManager == null) return;
        
        // Update phase text
        if (phaseText != null)
            phaseText.text = $"Phase: {gameManager.currentPhase}";
            
        // Update player text
        if (playerText != null)
            playerText.text = $"Current Player: {gameManager.currentPlayer + 1}";
            
        // Update timer text
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(gameManager.currentTurnTime)}s";
            
        // Update instructions based on phase
        if (instructionsText != null)
        {
            bool isHumanTurn = gameManager.IsHumanPlayer(gameManager.currentPlayer);
            
            switch (gameManager.currentPhase)
            {
                case GamePhase.Setup:
                    instructionsText.text = "Setup (vs Computer): Press 'T' to start Card Deployment Phase";
                    break;
                case GamePhase.CardDeployment:
                    if (isHumanTurn)
                        instructionsText.text = "Your Turn: Click deployment zones to place cards. Press 'A' to draw from deck, 'S' to select random card, 'N' to skip turn.";
                    else
                        instructionsText.text = "Computer is thinking about card placement...";
                    break;
                case GamePhase.BettingPhase:
                    if (isHumanTurn)
                        instructionsText.text = "Your Turn: Distribute coins among pieces. Press 'B' to auto-complete.";
                    else
                        instructionsText.text = "Computer is placing bets...";
                    break;
                case GamePhase.ChessBattle:
                    if (isHumanTurn)
                        instructionsText.text = "Your Turn: Click pieces to select, click tiles to move. Eliminate all enemies!";
                    else
                        instructionsText.text = "Computer is analyzing the board...";
                    break;
                case GamePhase.GameEnd:
                    instructionsText.text = "Game Over! Press 'R' to restart.";
                    break;
            }
        }
    }
    
    void Update()
    {
        UpdateUI();
        
        // Simple keyboard controls for testing
        if (gameManager != null)
        {
            // Only allow input if it's human player's turn
            bool canInput = gameManager.IsHumanPlayer(gameManager.currentPlayer);
            
            if (Input.GetKeyDown(KeyCode.T) && gameManager.currentPhase == GamePhase.Setup)
            {
                gameManager.StartCardDeploymentPhase();
            }
            else if (canInput && Input.GetKeyDown(KeyCode.A) && gameManager.currentPhase == GamePhase.CardDeployment)
            {
                // Draw a card from deck for current player
                if (gameManager.IsHumanPlayer(gameManager.currentPlayer))
                {
                    if (gameManager.cardSystem.TakeACardFromDeck(gameManager.currentPlayer))
                    {
                        Debug.Log("🎴 Drew a new card from deck!");
                    }
                    else
                    {
                        Debug.Log("Cannot draw from deck - hand is full or deck is empty!");
                    }
                }
            }
            else if (canInput && Input.GetKeyDown(KeyCode.S) && gameManager.currentPhase == GamePhase.CardDeployment)
            {
                // Select random card from current player's hand
                if (gameManager.IsHumanPlayer(gameManager.currentPlayer))
                {
                    gameManager.cardSystem.SelectRandomCardFromCurrentPlayerHand();
                }
            }
            else if (canInput && Input.GetKeyDown(KeyCode.N) && gameManager.currentPhase == GamePhase.CardDeployment)
            {
                gameManager.NextTurn();
            }
            else if (canInput && Input.GetKeyDown(KeyCode.B) && gameManager.currentPhase == GamePhase.BettingPhase)
            {
                gameManager.bettingSystem.AutoDistributeRemainingCoins(gameManager.currentPlayer);
                gameManager.NextTurn();
            }
            else if (Input.GetKeyDown(KeyCode.R) && gameManager.currentPhase == GamePhase.GameEnd)
            {
                gameManager.StartSetupPhase();
            }
        }
    }
}