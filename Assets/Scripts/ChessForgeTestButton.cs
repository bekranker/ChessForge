using UnityEngine;

public class ChessForgeTestButton : MonoBehaviour
{
    [Header("Test ChessForge")]
    [Tooltip("Click to test basic ChessForge functionality")]
    public bool testGame = false;
    
    void OnValidate()
    {
        if (testGame)
        {
            testGame = false;
            TestChessForge();
        }
    }
    
    [ContextMenu("Test ChessForge")]
    public void TestChessForge()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No GameManager found! Please run ChessForgeSetup first.");
            return;
        }
        
        Debug.Log("=== ChessForge Test ===");
        Debug.Log($"Current Phase: {gameManager.currentPhase}");
        Debug.Log($"Current Player: {gameManager.currentPlayer + 1}");
        Debug.Log($"Board Size: {gameManager.selectedBoardSize}");
        
        // Test starting the card deployment phase
        if (gameManager.currentPhase == GamePhase.Setup)
        {
            Debug.Log("Starting Card Deployment Phase...");
            gameManager.StartCardDeploymentPhase();
        }
        
        Debug.Log("ChessForge test completed!");
    }
}