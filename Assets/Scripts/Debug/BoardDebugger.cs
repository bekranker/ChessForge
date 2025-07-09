using UnityEngine;

public class BoardDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugDisplay = true;
    public KeyCode debugKey = KeyCode.F1;
    
    private BoardManager boardManager;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugBoardState();
        }
    }
    
    void DebugBoardState()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("🔍 No BoardManager found!");
            return;
        }
        
        Debug.Log("=== BOARD DEBUG INFO ===");
        Debug.Log($"📐 Board Size: {boardManager.boardWidth}x{boardManager.boardHeight}");
        
        // Check for all board tiles
        BoardTile[] allTiles = FindObjectsOfType<BoardTile>();
        Debug.Log($"🎲 Found {allTiles.Length} BoardTile components in scene");
        
        foreach (BoardTile tile in allTiles)
        {
            Vector3 worldPos = boardManager.GetWorldPosition(tile.x, tile.y);
            Debug.Log($"  Tile ({tile.x}, {tile.y}) -> World: {worldPos} | Object: {tile.gameObject.name}");
        }
        
        // Check for all pieces on board
        Debug.Log("🎭 Pieces on board:");
        for (int x = 0; x < boardManager.boardWidth; x++)
        {
            for (int y = 0; y < boardManager.boardHeight; y++)
            {
                ChessPiece piece = boardManager.GetPieceAt(x, y);
                if (piece != null)
                {
                    Vector3 pieceWorldPos = piece.transform.position;
                    Vector3 expectedWorldPos = boardManager.GetWorldPosition(x, y);
                    Debug.Log($"  Piece {piece.pieceType} at ({x}, {y}) -> Actual World: {pieceWorldPos} | Expected World: {expectedWorldPos}");
                }
            }
        }
        
        Debug.Log("=== END BOARD DEBUG ===");
    }
    
    void OnGUI()
    {
        if (!enableDebugDisplay) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        
        GUI.Label(new Rect(10, Screen.height - 100, 300, 20), $"Press {debugKey} for board debug info", style);
        
        if (boardManager != null)
        {
            GUI.Label(new Rect(10, Screen.height - 80, 300, 20), $"Board: {boardManager.boardWidth}x{boardManager.boardHeight}", style);
            
            BoardTile[] tiles = FindObjectsOfType<BoardTile>();
            GUI.Label(new Rect(10, Screen.height - 60, 300, 20), $"Tiles: {tiles.Length}", style);
            
            int pieceCount = 0;
            for (int x = 0; x < boardManager.boardWidth; x++)
            {
                for (int y = 0; y < boardManager.boardHeight; y++)
                {
                    if (boardManager.GetPieceAt(x, y) != null) pieceCount++;
                }
            }
            GUI.Label(new Rect(10, Screen.height - 40, 300, 20), $"Pieces: {pieceCount}", style);
        }
    }
}