using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardVisual : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Visual Components")]
    public Image cardBackground;
    public Image pieceIcon;
    public TextMeshProUGUI pieceNameText;
    public GameObject selectionHighlight;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color hoverColor = Color.cyan;
    public Color disabledColor = Color.gray;
    public Color draggingColor = new Color(1f, 1f, 1f, 0.6f);
    
    [Header("Drag Settings")]
    public Canvas dragCanvas;
    public float dragScale = 1.2f;
    public LayerMask boardLayerMask = -1; // For raycasting board tiles
    
    private Card cardData;
    private CardSystem cardSystem;
    private bool isSelected = false;
    private bool isInteractable = true;
    private bool isDragging = false;
    
    // Drag state
    private Vector3 originalPosition;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalScale;
    private Vector3 dragOffset;
    private Camera worldCamera;

    void Awake()
    {
        // Find components if not assigned
        if (cardBackground == null) cardBackground = GetComponent<Image>();
        if (pieceIcon == null) pieceIcon = transform.Find("PieceIcon")?.GetComponent<Image>();
        if (pieceNameText == null) pieceNameText = GetComponentInChildren<TextMeshProUGUI>();
        if (selectionHighlight == null) selectionHighlight = transform.Find("SelectionHighlight")?.gameObject;
        
        // Find CardSystem
        cardSystem = FindObjectOfType<CardSystem>();
        
        // Find the world space canvas
        if (dragCanvas == null)
        {
            dragCanvas = GetComponentInParent<Canvas>();
        }
        
        // Get the world camera for world space canvas
        worldCamera = Camera.main;
        if (worldCamera == null)
        {
            worldCamera = FindObjectOfType<Camera>();
        }
        
        // Ensure we have required components
        if (cardBackground == null)
        {
            cardBackground = gameObject.AddComponent<Image>();
        }
    }

    public void SetCard(Card card)
    {
        cardData = card;
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        if (cardData == null) return;
        
        // Set piece icon using CardSystem's sprite system
        if (pieceIcon != null && cardSystem != null)
        {
            pieceIcon.sprite = cardSystem.GetPieceSprite(cardData.pieceType);
            pieceIcon.color = cardData.playerIndex == 0 ? Color.white : Color.black;
        }
        else if (pieceIcon != null && cardData.CartIcon != null)
        {
            // Fallback to card's own icon
            pieceIcon.sprite = cardData.CartIcon;
        }
        
        // Set piece name
        if (pieceNameText != null)
        {
            pieceNameText.text = cardData.pieceType.ToString();
        }
        
        // Update interactability based on player and game state
        UpdateInteractability();
        
        // Update selection visual
        UpdateSelectionVisual();
    }
    
    void UpdateInteractability()
    {
        if (cardData == null) return;
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            isInteractable = false;
            return;
        }
        
        // Only allow interaction during card deployment phase
        if (gameManager.currentPhase != GamePhase.CardDeployment)
        {
            isInteractable = false;
            return;
        }
        
        // Only allow current player to interact with their cards
        if (cardData.playerIndex != gameManager.currentPlayer)
        {
            isInteractable = false;
            return;
        }
        
        // Only human player (Player 0) can interact
        if (cardData.playerIndex != 0)
        {
            isInteractable = false;
            return;
        }
        
        isInteractable = true;
    }
    
    public void UpdateSelectionVisual()
    {
        if (cardSystem == null) return;
        
        isSelected = (cardSystem.selectedCard == cardData);
        
        // Update highlight
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
        
        // Update card background color
        if (cardBackground != null)
        {
            Color targetColor;
            
            if (!isInteractable)
            {
                targetColor = disabledColor;
            }
            else if (isDragging)
            {
                targetColor = draggingColor;
            }
            else if (isSelected)
            {
                targetColor = selectedColor;
            }
            else
            {
                targetColor = normalColor;
            }
            
            cardBackground.color = targetColor;
        }
    }
    
    // DRAG AND DROP IMPLEMENTATION FOR WORLD SPACE CANVAS
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInteractable || cardData == null) return;
        
        Debug.Log($"🎯 Started dragging {cardData.pieceType} card");
        
        isDragging = true;
        
        // Store original state
        originalPosition = transform.position;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalScale = transform.localScale;
        
        // Calculate drag offset for world space canvas
        if (worldCamera != null && dragCanvas != null && dragCanvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, dragCanvas.planeDistance));
            dragOffset = transform.position - worldPos;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
        
        // Make card semi-transparent and scale up for visual feedback
        UpdateSelectionVisual();
        transform.localScale = originalScale * dragScale;
        
        // Ensure card renders on top during drag
        transform.SetAsLastSibling();
        
        // Disable raycasting on this card so we can detect drops below it
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        
        Debug.Log($"🎯 Drag offset calculated: {dragOffset}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || worldCamera == null) return;
        
        // Convert screen position to world position for world space canvas
        if (dragCanvas != null && dragCanvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, dragCanvas.planeDistance));
            transform.position = worldPos + dragOffset;
        }
        else
        {
            // Fallback for other canvas modes
            transform.position = worldCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, worldCamera.nearClipPlane + 1f));
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        Debug.Log($"🎯 Ended dragging {cardData.pieceType} card");
        
        isDragging = false;
        
        // Re-enable raycasting
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        
        // Check if we dropped on a valid board tile using 3D raycast for world objects
        bool placedSuccessfully = CheckForBoardDropWorldSpace(eventData);
        
        if (!placedSuccessfully)
        {
            // Return card to original position
            ReturnToOriginalPosition();
        }
        
        UpdateSelectionVisual();
    }
    
    bool CheckForBoardDropWorldSpace(PointerEventData eventData)
    {
        if (worldCamera == null) return false;
        
        // First try UI raycast for any UI elements
        var uiRaycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        
        foreach (var result in uiRaycastResults)
        {
            BoardTile boardTile = result.gameObject.GetComponent<BoardTile>();
            if (boardTile != null)
            {
                Debug.Log($"🎯 Dropped card on UI board tile at ({boardTile.x}, {boardTile.y})");
                return TryPlaceCardOnTile(boardTile);
            }
        }
        
        // Then try 3D raycast for world space board tiles
        Ray ray = worldCamera.ScreenPointToRay(eventData.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, boardLayerMask);
        
        foreach (RaycastHit hit in hits)
        {
            BoardTile boardTile = hit.collider.GetComponent<BoardTile>();
            if (boardTile != null)
            {
                Debug.Log($"🎯 Dropped card on world board tile at ({boardTile.x}, {boardTile.y})");
                return TryPlaceCardOnTile(boardTile);
            }
        }
        
        // Try 2D raycast for 2D board tiles
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);
        
        foreach (RaycastHit2D hit in hits2D)
        {
            BoardTile boardTile = hit.collider.GetComponent<BoardTile>();
            if (boardTile != null)
            {
                Debug.Log($"🎯 Dropped card on 2D board tile at ({boardTile.x}, {boardTile.y})");
                return TryPlaceCardOnTile(boardTile);
            }
        }
        
        Debug.Log($"❌ Did not drop on a valid board tile");
        return false;
    }
    
    bool TryPlaceCardOnTile(BoardTile boardTile)
    {
        Vector2Int position = new Vector2Int(boardTile.x, boardTile.y);
        
        Debug.Log($"🎯 Attempting to place {cardData.pieceType} at tile coordinates ({boardTile.x}, {boardTile.y})");
        
        if (cardSystem.TryPlaceCardAtPosition(cardData, position))
        {
            Debug.Log($"✅ Successfully placed {cardData.pieceType} at {position}");
            
            // Immediately destroy this card visual since it was played
            gameObject.SetActive(false); // Disable first to prevent any further interactions
            Destroy(gameObject, 0.1f); // Delay destruction slightly to ensure all operations complete
            return true;
        }
        else
        {
            Debug.Log($"❌ Cannot place {cardData.pieceType} at {position}");
            return false;
        }
    }
    
    void ReturnToOriginalPosition()
    {
        // Return to original parent and position
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        transform.position = originalPosition;
        transform.localScale = originalScale;
        
        Debug.Log($"↩️ Returned {cardData.pieceType} card to original position");
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        
        if (cardBackground != null)
        {
            cardBackground.color = hoverColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        UpdateSelectionVisual();
    }
    
    void Update()
    {
        // Continuously update interactability and selection state
        UpdateInteractability();
        UpdateSelectionVisual();
    }

    public Card GetCard()
    {
        return cardData;
    }
}