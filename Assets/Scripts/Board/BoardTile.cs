using UnityEngine;
using UnityEngine.EventSystems;

public class BoardTile : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int x, y;
    public BoardManager boardManager;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color validDropColor = Color.green;
    public Color invalidDropColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        // Add appropriate colliders for both 2D and 3D raycasting
        SetupColliders();

        // Get sprite renderer for visual feedback
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void SetupColliders()
    {
        // Add 2D collider for 2D physics raycast
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
            collider2D.isTrigger = true;
        }
    }

    public void Initialize(int x, int y, BoardManager manager)
    {
        this.x = x;
        this.y = y;
        this.boardManager = manager;
        
        Debug.Log($"🎲 BoardTile initialized at ({x}, {y}) for object {gameObject.name}");
    }

    // Handle UI drag and drop events
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"🎯 Card dropped on tile ({x}, {y})");

        // The CardVisual will handle the actual placement logic
        // This is just for visual feedback
        ResetTileColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Check if we're dragging a card
        if (eventData.pointerDrag != null)
        {
            CardVisual cardVisual = eventData.pointerDrag.GetComponent<CardVisual>();
            if (cardVisual != null)
            {
                // Show visual feedback for valid/invalid drop
                bool canPlace = CanPlaceCardHere(cardVisual.GetCard());
                SetTileColor(canPlace ? validDropColor : invalidDropColor);

                Debug.Log($"🎯 Hovering over tile ({x}, {y}) - Can place: {canPlace}");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset tile color when drag exits
        ResetTileColor();
    }

    bool CanPlaceCardHere(Card card)
    {
        if (card == null || boardManager == null) return false;

        return boardManager.CanPlacePieceAt(x, y, card.playerIndex);
    }

    void SetTileColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    void ResetTileColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // Keep mouse click functionality for backwards compatibility

    void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            Vector2Int position = new Vector2Int(x, y);

            switch (GameManager.Instance.currentPhase)
            {
                case GamePhase.CardDeployment:
                    HandleCardDeploymentClick(position);
                    break;
                case GamePhase.ChessBattle:
                    HandleChessBattleClick(position);
                    break;
            }
        }
    }

    void HandleCardDeploymentClick(Vector2Int position)
    {
        // Get the card system and try to place selected card
        CardSystem cardSystem = FindObjectOfType<CardSystem>();
        if (cardSystem != null)
        {
            cardSystem.TryPlaceCardAt(position);
        }
    }

    void HandleChessBattleClick(Vector2Int position)
    {
        // Get the chess combat system and handle piece selection/movement
        ChessCombat chessCombat = FindObjectOfType<ChessCombat>();
        if (chessCombat != null)
        {
            chessCombat.HandleTileClick(position);
        }
    }

}