using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject winLabel;
    [SerializeField] private GameObject loseLabel;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button restartButton;
    
    [Header("Phase Display")]
    [SerializeField] private GameObject setupPhaseUI;
    [SerializeField] private GameObject bettingPhaseUI;
    [SerializeField] private GameObject playingPhaseUI;
    [SerializeField] private GameObject endPhaseUI;
    
    private ChessGameManager gameManager;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<ChessGameManager>();
        
        if (gameManager != null)
        {
            // Subscribe to game manager events
            gameManager.OnPhaseChanged += UpdatePhaseDisplay;
            gameManager.OnPlayerChanged += UpdatePlayerDisplay;
            gameManager.OnTimerUpdated += UpdateTimerDisplay;
        }
        
        // Initialize UI
        if (winLabel != null) winLabel.SetActive(false);
        if (loseLabel != null) loseLabel.SetActive(false);
        
        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
        
        InitializePhaseUI();
    }
    
    private void InitializePhaseUI()
    {
        // Hide all phase UIs initially
        if (setupPhaseUI != null) setupPhaseUI.SetActive(false);
        if (bettingPhaseUI != null) bettingPhaseUI.SetActive(false);
        if (playingPhaseUI != null) playingPhaseUI.SetActive(false);
        if (endPhaseUI != null) endPhaseUI.SetActive(false);
    }
    
    private void UpdatePhaseDisplay(GamePhases phase)
    {
        // Update phase text
        if (phaseText != null && gameManager != null)
        {
            phaseText.text = gameManager.GetCurrentPhaseText();
        }
        
        // Show/hide appropriate phase UI
        InitializePhaseUI();
        
        switch (phase)
        {
            case GamePhases.Setup:
                if (setupPhaseUI != null) setupPhaseUI.SetActive(true);
                break;
            case GamePhases.Betting:
                if (bettingPhaseUI != null) bettingPhaseUI.SetActive(true);
                break;
            case GamePhases.Playing:
                if (playingPhaseUI != null) playingPhaseUI.SetActive(true);
                break;
            case GamePhases.Ended:
                if (endPhaseUI != null) endPhaseUI.SetActive(true);
                if (restartButton != null) restartButton.gameObject.SetActive(true);
                break;
        }
    }
    
    private void UpdatePlayerDisplay(PlayerColors player)
    {
        if (playerText != null && gameManager != null)
        {
            playerText.text = gameManager.GetCurrentPlayerText();
            
            // Change color based on player
            if (player == PlayerColors.White)
            {
                playerText.color = Color.white;
            }
            else
            {
                playerText.color = Color.black;
            }
        }
    }
    
    private void UpdateTimerDisplay(float time)
    {
        if (timerText != null)
        {
            if (time > 0)
            {
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
                
                // Change color when time is running low
                if (time < 10f)
                {
                    timerText.color = Color.red;
                }
                else if (time < 30f)
                {
                    timerText.color = Color.yellow;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
            else
            {
                timerText.text = "";
            }
        }
    }
    
    public void ShowWinLabel()
    {
        if (winLabel != null)
        {
            winLabel.SetActive(true);
        }
        if (loseLabel != null)
        {
            loseLabel.SetActive(false);
        }
    }
    
    public void ShowLoseLabel()
    {
        if (loseLabel != null)
        {
            loseLabel.SetActive(true);
        }
        if (winLabel != null)
        {
            winLabel.SetActive(false);
        }
    }
    
    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
    
    // Button handlers for betting phase
    public void OnFinishedBetting()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerAction();
        }
    }
    
    // Button handlers for setup phase
    public void OnFinishedSetup()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerAction();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnPhaseChanged -= UpdatePhaseDisplay;
            gameManager.OnPlayerChanged -= UpdatePlayerDisplay;
            gameManager.OnTimerUpdated -= UpdateTimerDisplay;
        }
    }
}