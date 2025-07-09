using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Board Visualization")]
    [Header("Tile Prefabs")]
    public GameObject whiteTilePrefab;
    public GameObject blackTilePrefab;

    [Header("Legacy Tile Settings (used if prefabs not assigned)")]
    public GameObject tilePrefab;
    public Color whiteColor = Color.white;
    public Color blackColor = new Color(0.4f, 0.2f, 0.1f);
    public Color player1DeployColor = new Color(0.8f, 0.9f, 1f);
    public Color player2DeployColor = new Color(1f, 0.9f, 0.8f);
    public Color highlightColor = Color.yellow;
    public Color validMoveColor = Color.green;

    [Header("Board State")]
    public BoardSize currentBoardSize;
    public int boardWidth;
    public int boardHeight;
    
    [Header("Tile Layout")]
    public float tileSize = 1f;        // Visual size/scale of each tile
    public float tileSpacing = 1f;     // Distance between tile centers

    private GameManager gameManager;
    private GameObject[,] tiles;
    private ChessPiece[,] pieces;
    private GameObject tilesParent;
    public GameObject piecesParent;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void SetupBoard(BoardSize size)
    {
        currentBoardSize = size;
        boardWidth = (int)size;
        boardHeight = (int)size;

        ClearBoard();
        CreateTiles();
        InitializePieceArray();

        // Debug deployment zones
        int deploymentRows = GetPlayerDeploymentRows();
        Debug.Log($"Board setup complete: {boardWidth}x{boardHeight}");
        Debug.Log($"Player deployment rows: {deploymentRows}");
        Debug.Log($"Player 1 deployment zone: rows 0-{deploymentRows - 1}");
        Debug.Log($"Player 2 deployment zone: rows {boardHeight - deploymentRows}-{boardHeight - 1}");
        Debug.Log($"Neutral zone: rows {deploymentRows}-{boardHeight - deploymentRows - 1}");
    }

    void ClearBoard()
    {
        if (tilesParent != null)
            DestroyImmediate(tilesParent);
        if (piecesParent != null)
            DestroyImmediate(piecesParent);

        tilesParent = new GameObject("Board Tiles");
        tilesParent.transform.SetParent(transform);

        piecesParent = new GameObject("Board Pieces");
        piecesParent.transform.SetParent(transform);
    }

    void CreateTiles()
    {
        tiles = new GameObject[boardWidth, boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                CreateTile(x, y);
            }
        }

        // Center the board
        Vector3 boardCenter = new Vector3((boardWidth - 1) * tileSpacing * 0.5f, (boardHeight - 1) * tileSpacing * 0.5f, 0);
        tilesParent.transform.position = -boardCenter;
        piecesParent.transform.position = -boardCenter;
    }

    void CreateTile(int x, int y)
    {
        GameObject tile;
        bool isWhiteTile = (x + y) % 2 == 0;

        // Use custom prefabs if available
        if (whiteTilePrefab != null && blackTilePrefab != null)
        {
            GameObject prefabToUse = isWhiteTile ? whiteTilePrefab : blackTilePrefab;
            tile = Instantiate(prefabToUse, tilesParent.transform);
        }
        else if (tilePrefab != null)
        {
            // Fallback to legacy single prefab
            tile = Instantiate(tilePrefab, tilesParent.transform);
        }
        else
        {
            // Create 2D sprite tile instead of 3D quad
            tile = new GameObject($"Tile_{x}_{y}");
            tile.transform.SetParent(tilesParent.transform);

            // Add SpriteRenderer for 2D
            SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateTileSprite();
            spriteRenderer.sortingOrder = 0;
        }

        tile.name = $"Tile_{x}_{y}";
        tile.transform.position = new Vector3(x * tileSpacing, y * tileSpacing, 0);
        tile.transform.localScale = Vector3.one * tileSize;

        // Add 2D collider for interaction
        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = tile.AddComponent<BoxCollider2D>();
        }

        // Add tile component for interaction
        BoardTile tileComponent = tile.GetComponent<BoardTile>();
        if (tileComponent == null)
        {
            tileComponent = tile.AddComponent<BoardTile>();
        }

        tileComponent.Initialize(x, y, this);

        // Only apply color tinting if using legacy tiles (not custom prefabs)
        if (whiteTilePrefab == null || blackTilePrefab == null)
        {
            SetTileColor(x, y, GetDefaultTileColor(x, y));
        }
        else
        {
            // For custom prefabs, apply deployment zone coloring if needed
            ApplyDeploymentZoneEffect(x, y, tile);
        }

        tiles[x, y] = tile;
    }

    Color GetDefaultTileColor(int x, int y)
    {
        bool isWhite = (x + y) % 2 == 0;

        // Check if this is in a player deployment zone
        if (IsPlayer1DeploymentZone(x, y))
        {
            return Color.Lerp(isWhite ? whiteColor : blackColor, player1DeployColor, 0.3f);
        }
        else if (IsPlayer2DeploymentZone(x, y))
        {
            return Color.Lerp(isWhite ? whiteColor : blackColor, player2DeployColor, 0.3f);
        }

        return isWhite ? whiteColor : blackColor;
    }

    void ApplyDeploymentZoneEffect(int x, int y, GameObject tile)
    {
        // Add a subtle overlay or effect for deployment zones when using custom prefabs
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color deploymentTint = Color.clear;

            if (IsPlayer1DeploymentZone(x, y))
            {
                deploymentTint = player1DeployColor;
            }
            else if (IsPlayer2DeploymentZone(x, y))
            {
                deploymentTint = player2DeployColor;
            }

            if (deploymentTint != Color.clear)
            {
                // Apply a subtle tint to show deployment zones
                Color currentColor = spriteRenderer.color;
                spriteRenderer.color = Color.Lerp(currentColor, deploymentTint, 0.2f);
            }
        }
    }

    void InitializePieceArray()
    {
        pieces = new ChessPiece[boardWidth, boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                pieces[x, y] = null;
            }
        }
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < boardWidth && y >= 0 && y < boardHeight;
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return IsValidPosition(position.x, position.y);
    }

    public bool IsPlayer1DeploymentZone(int x, int y)
    {
        int playerRows = GetPlayerDeploymentRows();
        return y < playerRows;
    }

    public bool IsPlayer2DeploymentZone(int x, int y)
    {
        int playerRows = GetPlayerDeploymentRows();
        return y >= (boardHeight - playerRows);
    }
    
    public int GetPlayerDeploymentRows()
    {
        // Calculate deployment rows based on board size
        // Each player gets a maximum of boardSize/3 rows (minimum 1)
        // This leaves middle rows unplaceable
        int deploymentRows = Mathf.Max(1, boardWidth / 3);
        
        // For odd board sizes, ensure we don't overlap
        if (boardWidth % 2 == 1 && deploymentRows * 2 >= boardWidth)
        {
            deploymentRows = (boardWidth - 1) / 2;
        }
        
        return deploymentRows;
    }

    public bool CanPlacePieceAt(int x, int y, int playerIndex)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.Log($"Invalid position: ({x}, {y}) - out of bounds");
            return false;
        }

        if (GetPieceAt(x, y) != null)
        {
            Debug.Log($"Position ({x}, {y}) already occupied");
            return false;
        }

        // Check deployment zones during card deployment phase
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GamePhase.CardDeployment)
        {
            if (playerIndex == 0 && !IsPlayer1DeploymentZone(x, y))
            {
                Debug.Log($"Player 1 cannot place pieces at ({x}, {y}) - outside deployment zone");
                return false;
            }
            if (playerIndex == 1 && !IsPlayer2DeploymentZone(x, y))
            {
                Debug.Log($"Player 2 cannot place pieces at ({x}, {y}) - outside deployment zone");
                return false;
            }
        }

        return true;
    }

    public ChessPiece GetPieceAt(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return null;
        return pieces[x, y];
    }

    public ChessPiece GetPieceAt(Vector2Int position)
    {
        return GetPieceAt(position.x, position.y);
    }

    public void SetPieceAt(int x, int y, ChessPiece piece)
    {
        if (!IsValidPosition(x, y))
            return;

        pieces[x, y] = piece;

        if (piece != null)
        {
            piece.boardPosition = new Vector2Int(x, y);
            piece.transform.position = GetWorldPosition(x, y);
        }
    }

    public void SetPieceAt(Vector2Int position, ChessPiece piece)
    {
        SetPieceAt(position.x, position.y, piece);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 boardOffset = -new Vector3((boardWidth - 1) * tileSpacing * 0.5f, (boardHeight - 1) * tileSpacing * 0.5f, 0);
        return new Vector3(x * tileSpacing, y * tileSpacing, -0.1f) + boardOffset;
    }

    public Vector3 GetWorldPosition(Vector2Int position)
    {
        return GetWorldPosition(position.x, position.y);
    }

    public Vector2Int GetBoardPosition(Vector3 worldPosition)
    {
        Vector3 boardOffset = new Vector3((boardWidth - 1) * tileSpacing * 0.5f, (boardHeight - 1) * tileSpacing * 0.5f, 0);
        Vector3 localPos = worldPosition + boardOffset;

        int x = Mathf.RoundToInt(localPos.x / tileSpacing);
        int y = Mathf.RoundToInt(localPos.y / tileSpacing);

        return new Vector2Int(x, y);
    }

    public void SetTileColor(int x, int y, Color color)
    {
        if (!IsValidPosition(x, y) || tiles[x, y] == null)
            return;

        SpriteRenderer spriteRenderer = tiles[x, y].GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    // Create a simple square sprite for tiles
    Sprite CreateTileSprite()
    {
        // Create a 64x64 white texture
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    public void ResetTileColors()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                SetTileColor(x, y, GetDefaultTileColor(x, y));
            }
        }
    }

    public void HighlightTile(int x, int y, Color color)
    {
        SetTileColor(x, y, color);
    }

    public void HighlightValidMoves(List<Vector2Int> moves)
    {
        foreach (Vector2Int move in moves)
        {
            HighlightTile(move.x, move.y, validMoveColor);
        }
    }

    public void MovePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece piece = GetPieceAt(from);
        ChessPiece capturedPiece = GetPieceAt(to);

        if (piece == null)
        {
            Debug.LogWarning($"No piece found at {from} to move!");
            return;
        }

        // Remove captured piece
        if (capturedPiece != null)
        {
            Debug.Log($"CAPTURE: {piece.pieceType} (Player {piece.playerIndex + 1}) captures {capturedPiece.pieceType} (Player {capturedPiece.playerIndex + 1}) at {to}");
            SetPieceAt(to, null);
            Destroy(capturedPiece.gameObject);
        }
        else
        {
            Debug.Log($"MOVE: {piece.pieceType} (Player {piece.playerIndex + 1}) moves from {from} to {to}");
        }

        // Move piece
        SetPieceAt(from, null);
        SetPieceAt(to, piece);
        piece.MoveTo(to);

        ResetTileColors();
    }

    public List<ChessPiece> GetAllPiecesForPlayer(int playerIndex)
    {
        List<ChessPiece> playerPieces = new List<ChessPiece>();

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                ChessPiece piece = GetPieceAt(x, y);
                if (piece != null && piece.playerIndex == playerIndex)
                {
                    playerPieces.Add(piece);
                }
            }
        }

        return playerPieces;
    }

    public int CountPiecesForPlayer(int playerIndex)
    {
        return GetAllPiecesForPlayer(playerIndex).Count;
    }
}