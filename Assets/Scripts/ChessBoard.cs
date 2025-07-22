using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;


public class ChessBoard : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardSize;
    public float tileSize = 1f;
    [SerializeField] private bool autoCalculateTileSize = true;
    public Vector3 boardOffset = Vector3.zero;
    [SerializeField] private GameObject _whiteTilePrefab;
    [SerializeField] private GameObject _blackTilePrefab;

    [Header("Animation Settings")]
    [SerializeField] private float tileAnimationDelay = 0.05f;
    [SerializeField] private float punchStrength = 0.3f;
    [SerializeField] private float punchDuration = 0.6f;

    [Header("Visual")]
    public GameObject highlightPrefab;
    public Color FreeTileColor, OccupiedTileColor;

    public ChessPiece[,] board;
    private List<GameObject> highlightedSquares = new List<GameObject>();
    private List<TileConfig> tiles = new List<TileConfig>();
    private PlayerColors currentPlayer = PlayerColors.White;

    public void Start()
    {
        boardSize = PlayerPrefs.GetInt("BoardSize", 0);
        InitializeBoard();
        CreateTiles();
    }

    private void InitializeBoard()
    {
        board = new ChessPiece[boardSize, boardSize];
    }
    private void CreateTiles()
    {
        if (_whiteTilePrefab == null || _blackTilePrefab == null)
        {
            Debug.LogWarning("White or Black tile prefab is not assigned!");
            return;
        }

        // Clear existing tiles
        ClearTiles();

        // Auto-calculate tile size based on sprite renderer bounds
        if (autoCalculateTileSize)
        {
            CalculateOptimalTileSize();
        }

        // Get board size from PlayerPrefs (3x3 to 8x8)
        int size = boardSize;
        // Calculate center position to center the board
        float halfSize = (size - 1) * tileSize * 0.5f;
        Vector3 centerOffset = new Vector3(-halfSize, -halfSize, 0);

        // Create tiles with animation
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector3 tilePosition = new Vector3(
                    x * tileSize + centerOffset.x + boardOffset.x,
                    y * tileSize + centerOffset.y + boardOffset.y,
                    boardOffset.z
                );

                // Determine tile color based on chess board pattern
                bool isWhiteTile = (x + y) % 2 == 0;

                GameObject tilePrefab = isWhiteTile ? _whiteTilePrefab : _blackTilePrefab;
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
                TileConfig tileConfig = new TileConfig(new Vector2Int(x, y), false, tile, isWhiteTile);
                tiles.Add(tileConfig);

                // Set initial scale to zero for punch animation
                tile.transform.localScale = Vector3.zero;

                // Calculate delay based on distance from center for wave effect
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f));
                float delay = distanceFromCenter * tileAnimationDelay;

                // Apply DOTween punch scale animation
                tile.transform.DOScale(Vector3.one, punchDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        // Add a subtle punch effect after the initial scale
                        tile.transform.DOPunchScale(Vector3.one * punchStrength, 0.3f, 1, 0.5f);
                    });

                // Set tile name for debugging (include color info)
                string tileColor = isWhiteTile ? "White" : "Black";
                tile.name = $"Tile_{x}_{y}_{tileColor}";
            }
        }

        // Update board array size to match new board size
        board = new ChessPiece[size, size];
    }

    private void ClearTiles()
    {
        foreach (TileConfig tile in tiles)
        {
            if (tile != null)
            {
                tile.ClearTile();
            }
        }
        tiles.Clear();
    }

    private void CalculateOptimalTileSize()
    {
        // Get the actual size of the tile sprites to ensure no gaps
        SpriteRenderer whiteSpriteRenderer = _whiteTilePrefab.GetComponent<SpriteRenderer>();
        if (whiteSpriteRenderer != null && whiteSpriteRenderer.sprite != null)
        {
            // Use the sprite's bounds to get the actual rendered size
            tileSize = whiteSpriteRenderer.sprite.bounds.size.x;
            Debug.Log($"Auto-calculated tile size: {tileSize}");
        }
        else
        {
            Debug.LogWarning("Could not auto-calculate tile size. Using manual tileSize value.");
        }
    }

    public TileConfig FindTile(GameObject tileObject)
    {
        foreach (TileConfig tile in tiles)
        {
            if (tile != null && tile.TileObject == tileObject)
            {
                return tile;
            }
        }
        return null;
    }
    public Vector3 BoardToWorldPosition(Vector2Int boardPosition)
    {
        return new Vector3(
            boardPosition.x * tileSize + boardOffset.x,
            boardPosition.y * tileSize + boardOffset.y,
            boardOffset.z
        );
    }

    public Vector2Int WorldToBoardPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt((worldPosition.x - boardOffset.x) / tileSize),
            Mathf.RoundToInt((worldPosition.y - boardOffset.y) / tileSize)
        );
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardSize &&
               position.y >= 0 && position.y < boardSize;
    }

    public ChessPiece GetPieceAt(Vector2Int position)
    {
        if (!IsValidPosition(position))
            return null;

        return board[position.x, position.y];
    }

    public void UpdatePiecePosition(ChessPiece piece, Vector2Int oldPosition, Vector2Int newPosition)
    {
        if (IsValidPosition(oldPosition))
        {
            board[oldPosition.x, oldPosition.y] = null;
        }

        if (IsValidPosition(newPosition))
        {
            board[newPosition.x, newPosition.y] = piece;
        }
    }

    public void RemovePiece(ChessPiece piece)
    {
        Vector2Int position = piece.boardPosition;
        if (IsValidPosition(position))
        {
            board[position.x, position.y] = null;
        }
    }

    public List<ChessPiece> GetPiecesOfColor(PlayerColors color)
    {
        List<ChessPiece> pieces = new List<ChessPiece>();

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                ChessPiece piece = board[x, y];
                if (piece != null && piece.pieceColor == color)
                {
                    pieces.Add(piece);
                }
            }
        }

        return pieces;
    }

    public void HighlightValidMoves(List<Vector2Int> validMoves)
    {
        ClearHighlights();

        if (highlightPrefab == null) return;

        foreach (Vector2Int move in validMoves)
        {
            Vector3 worldPosition = BoardToWorldPosition(move);
            GameObject highlight = Instantiate(highlightPrefab, worldPosition, Quaternion.identity, transform);
            highlightedSquares.Add(highlight);
        }
    }

    public void ClearHighlights()
    {
        foreach (GameObject highlight in highlightedSquares)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }
        highlightedSquares.Clear();
    }

    public void SwitchCurrentPlayer()
    {
        currentPlayer = currentPlayer == PlayerColors.White ?
                       PlayerColors.Black : PlayerColors.White;
    }

    public bool IsInCheck(PlayerColors kingColor)
    {
        ChessPiece king = FindKing(kingColor);
        if (king == null) return false;

        PlayerColors enemyColor = kingColor == PlayerColors.White ?
                                          PlayerColors.Black : PlayerColors.White;

        List<ChessPiece> enemyPieces = GetPiecesOfColor(enemyColor);

        foreach (ChessPiece enemyPiece in enemyPieces)
        {
            if (enemyPiece.IsAttackingSquare(king.boardPosition))
            {
                return true;
            }
        }

        return false;
    }

    public ChessPiece FindKing(PlayerColors color)
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                ChessPiece piece = board[x, y];
                if (piece != null && piece.pieceType == ChessPiece.PieceType.King && piece.pieceColor == color)
                {
                    return piece;
                }
            }
        }

        return null;
    }

    public List<TileConfig> GetAvailablePlacementTiles()
    {
        // Determine how many rows player can place pieces based on board size
        int allowedRows = GetAllowedPlacementRows();
        List<TileConfig> availableTiles = new List<TileConfig>();
        // Search through tiles for free positions in allowed rows
        foreach (TileConfig tile in tiles)
        {
            if (tile != null)
            {
                if (tile.Position.y < allowedRows)
                {
                    if (tile.SetFreePosition(FreeTileColor, OccupiedTileColor))
                    {
                        availableTiles.Add(tile);
                    }
                }
            }
        }
        return availableTiles;
    }
    public void ClearAvailablePlacementTiles()
    {
        // Reset all tiles to their default state
        foreach (TileConfig tile in tiles)
        {
            if (tile != null)
            {
                tile.ReturnDefaultColor();
            }
        }
    }
    private int GetAllowedPlacementRows()
    {
        // Rules for allowed placement rows based on board size:
        // half the board size rounded up

        return Mathf.CeilToInt(boardSize / 2f) - (boardSize % 2 == 0 ? 0 : 1);
    }
}