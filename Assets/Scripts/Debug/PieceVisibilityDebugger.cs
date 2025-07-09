using UnityEngine;

public class PieceVisibilityDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    public KeyCode forceRevealKey = KeyCode.F2;
    public KeyCode debugVisibilityKey = KeyCode.F3;
    
    private GameManager gameManager;
    private CardSystem cardSystem;
    private BoardManager boardManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        cardSystem = FindObjectOfType<CardSystem>();
        boardManager = FindObjectOfType<BoardManager>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(forceRevealKey))
        {
            ForceRevealAllPieces();
        }
        
        if (Input.GetKeyDown(debugVisibilityKey))
        {
            DebugPieceVisibility();
        }
    }
    
    void ForceRevealAllPieces()
    {
        Debug.Log("🔧 FORCE REVEAL: Manually revealing all pieces");
        
        if (cardSystem != null)
        {
            cardSystem.RevealAllPieces();
        }
        else
        {
            Debug.LogError("❌ CardSystem not found!");
        }
    }
    
    void DebugPieceVisibility()
    {
        if (boardManager == null)
        {
            Debug.LogError("❌ BoardManager not found!");
            return;
        }
        
        Debug.Log("=== PIECE VISIBILITY DEBUG ===");
        Debug.Log($"🎮 Current Game Phase: {gameManager?.currentPhase}");
        
        // Check all pieces on the board
        for (int x = 0; x < boardManager.boardWidth; x++)
        {
            for (int y = 0; y < boardManager.boardHeight; y++)
            {
                ChessPiece piece = boardManager.GetPieceAt(x, y);
                if (piece != null)
                {
                    SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        float alpha = spriteRenderer.color.a;
                        string visibilityStatus = alpha > 0.5f ? "VISIBLE" : "HIDDEN";
                        string playerName = piece.playerIndex == 0 ? "Player" : "Computer";
                        
                        Debug.Log($"🎭 {piece.pieceType} ({playerName}) at ({x},{y}): {visibilityStatus} (Alpha: {alpha:F2})");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Piece at ({x},{y}) has no SpriteRenderer!");
                    }
                }
            }
        }
        
        Debug.Log("=== END VISIBILITY DEBUG ===");
    }
    
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;
        
        float yPos = Screen.height - 200;
        
        GUI.Label(new Rect(10, yPos, 400, 20), $"Press {forceRevealKey} to force reveal all pieces", style);
        GUI.Label(new Rect(10, yPos + 20, 400, 20), $"Press {debugVisibilityKey} to debug piece visibility", style);
        
        if (gameManager != null)
        {
            GUI.Label(new Rect(10, yPos + 40, 400, 20), $"Current Phase: {gameManager.currentPhase}", style);
        }
    }
}