using UnityEngine;
using System.Collections.Generic;

public class CardSystem : MonoBehaviour
{
    [Header("Card Management")]
    public List<Card>[] playerHands;
    public Card selectedCard;
    public int maxHandSize = 3;

    [Header("Card Prefabs")]
    public GameObject cardPrefab; // Legacy fallback prefab

    [Header("Player Piece Prefabs")]
    public GameObject[] whiteCardPrefabs; // White pieces: Pawn, Rook, Knight, Bishop, Queen, King
    public GameObject[] blackCardPrefabs; // Black pieces: Pawn, Rook, Knight, Bishop, Queen, King
    
    [Header("Piece Sprites")]
    public Sprite[] pieceSprites; // Array of piece sprites: [0] = Pawn, [1] = Rook, [2] = Knight, [3] = Bishop, [4] = Queen, [5] = King

    [Header("Deck System")]
    public GameObject cardVisualPrefab; // Visual card prefab that spawns and moves to hand
    public Transform deckSpawnPoint;    // Where cards spawn from (deck location)
    public Transform[] playerHandAreas; // Where cards move to (bottom of screen for each player)
    public float cardMoveSpeed = 5f;    // Speed of card animation to hand

    [Header("Slot System")]
    public GameObject slotPrefab;       // UI prefab for card slots
    public Transform[] playerSlotParents; // UI containers where slots will be created for each player
    public float slotSpacing = 120f;    // Spacing between slots in pixels

    private List<GameObject>[] playerSlots; // Dynamic slots for each player

    [Header("Deck Contents")]
    [SerializeField] private Dictionary<PieceType, int>[] playerDeckContents = new Dictionary<PieceType, int>[2];
    public int initialPawnCount = 8;
    public int initialRookCount = 2;
    public int initialKnightCount = 2;
    public int initialBishopCount = 2;
    public int initialQueenCount = 1;
    public int initialKingCount = 1;

    private GameManager gameManager;
    private BoardManager boardManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        boardManager = manager.boardManager;

        // Initialize player hands
        playerHands = new List<Card>[2];
        playerHands[0] = new List<Card>();
        playerHands[1] = new List<Card>();

        // Initialize player slots
        playerSlots = new List<GameObject>[2];
        playerSlots[0] = new List<GameObject>();
        playerSlots[1] = new List<GameObject>();

        // Initialize deck
        InitializeDeck();
    }

    public void InitializeCardPhase()
    {
        // Clear existing hands
        ClearAllHands();

        // Each player starts with one card in hand (they decide whether to place it or not)
        DrawCardForPlayer(0);
        DrawCardForPlayer(1);

        // Spawn visual card for the initial human player card
        if (cardVisualPrefab != null)
        {
            StartCoroutine(AnimateCardToHand(playerHands[0][0], 0));
        }

        UpdateHandUI();
    }

    void ClearAllHands()
    {
        for (int i = 0; i < 2; i++)
        {
            playerHands[i].Clear();
        }
        selectedCard = null;
        UpdateHandUI();
    }

    public bool DrawCardForPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 2)
            return false;

        List<Card> hand = playerHands[playerIndex];

        // Check if hand is full
        if (hand.Count >= maxHandSize)
        {
            Debug.Log($"Player {playerIndex + 1}'s hand is full! Cannot draw more cards.");
            return false;
        }

        // Draw random card based on rarity weights
        PieceType drawnPieceType = gameManager.GetGameConfig().DrawRandomCard();

        // Create card
        Card newCard = new Card
        {
            pieceType = drawnPieceType,
            playerIndex = playerIndex,
            id = System.Guid.NewGuid().ToString()
        };

        hand.Add(newCard);

        Debug.Log($"Player {playerIndex + 1} drew a {drawnPieceType} card!");
        UpdateHandUI();

        return true;
    }

    public void SelectCard(Card card)
    {
        // Only allow selecting cards during card deployment phase
        if (gameManager.currentPhase != GamePhase.CardDeployment)
            return;

        // Only allow current player to select their cards
        if (card.playerIndex != gameManager.currentPlayer)
            return;

        selectedCard = card;
        UpdateHandUI();

        Debug.Log($"Selected {card.pieceType} card");
    }

    public bool SelectRandomCardFromCurrentPlayerHand()
    {
        // Only allow during card deployment phase
        if (gameManager.currentPhase != GamePhase.CardDeployment)
        {
            Debug.Log("Can only select cards during Card Deployment phase!");
            return false;
        }

        int currentPlayerIndex = gameManager.currentPlayer;
        List<Card> currentPlayerHand = playerHands[currentPlayerIndex];

        // Check if player has any cards
        if (currentPlayerHand.Count == 0)
        {
            Debug.Log($"Player {currentPlayerIndex + 1} has no cards in hand!");
            return false;
        }

        // Select random card from hand
        int randomIndex = Random.Range(0, currentPlayerHand.Count);
        Card randomCard = currentPlayerHand[randomIndex];

        SelectCard(randomCard);

        Debug.Log($"🎲 Player {currentPlayerIndex + 1} randomly selected: {randomCard.pieceType}");
        return true;
    }

    public bool TryPlaceCardAt(Vector2Int position)
    {
        if (gameManager.currentPhase != GamePhase.CardDeployment)
            return false;

        if (selectedCard == null)
        {
            Debug.Log("No card selected!");
            return false;
        }

        // Check if position is valid for placement
        if (!boardManager.CanPlacePieceAt(position.x, position.y, gameManager.currentPlayer))
        {
            Debug.Log("Cannot place piece at that position!");
            return false;
        }

        // Create piece from card
        CreatePieceFromCard(selectedCard, position);

        // Destroy the visual card UI element when played
        if (selectedCard.visualGameObject != null)
        {
            Debug.Log($"🗑️ Destroying hand card visual: {selectedCard.visualGameObject.name}");
            Destroy(selectedCard.visualGameObject);
            selectedCard.visualGameObject = null;
        }

        // Remove card from hand
        playerHands[selectedCard.playerIndex].Remove(selectedCard);
        selectedCard = null;

        UpdateHandUI();

        // End turn
        gameManager.NextTurn();

        return true;
    }
    GameObject pieceObject;
    void CreatePieceFromCard(Card card, Vector2Int position)
    {
        // Select appropriate prefab array based on player
        GameObject[] selectedPrefabs = card.playerIndex == 0 ? whiteCardPrefabs : blackCardPrefabs;

        // Use specific prefab for piece type if available
        if (selectedPrefabs != null && selectedPrefabs.Length > (int)card.pieceType && selectedPrefabs[(int)card.pieceType] != null)
        {
            pieceObject = Instantiate(selectedPrefabs[(int)card.pieceType], boardManager.piecesParent.transform);
        }
        if (cardPrefab != null)
        {
            // Fallback to generic card prefab
            pieceObject = Instantiate(cardPrefab, boardManager.piecesParent.transform);
        }
        else
        {
            // Create basic 2D sprite object
            pieceObject = new GameObject($"{card.pieceType}_{card.playerIndex}_{position.x}_{position.y}");
            pieceObject.transform.SetParent(boardManager.piecesParent.transform);
        }

        pieceObject.name = $"{card.pieceType}_{card.playerIndex}_{position.x}_{position.y}";

        // Add or get piece component
        ChessPiece pieceComponent = GetOrAddPieceComponent(pieceObject, card.pieceType);

        // Initialize piece
        pieceComponent.Initialize(card.pieceType, card.playerIndex, position, boardManager, gameManager);

        // Set visual representation
        SetupPieceVisuals(pieceObject, card);

        // Place on board
        boardManager.SetPieceAt(position, pieceComponent);

        // Update visibility based on current game phase
        UpdateSinglePieceVisibility(pieceComponent);

        Debug.Log($"Placed {card.pieceType} at {position}");
    }

    ChessPiece GetOrAddPieceComponent(GameObject pieceObject, PieceType type)
    {
        // Check if prefab already has a chess piece component
        ChessPiece existingComponent = pieceObject.GetComponent<ChessPiece>();
        if (existingComponent != null)
            return existingComponent;

        // Add appropriate component based on piece type
        switch (type)
        {
            case PieceType.Pawn: return pieceObject.AddComponent<Pawn>();
            case PieceType.Rook: return pieceObject.AddComponent<Rook>();
            case PieceType.Knight: return pieceObject.AddComponent<Knight>();
            case PieceType.Bishop: return pieceObject.AddComponent<Bishop>();
            case PieceType.Queen: return pieceObject.AddComponent<Queen>();
            case PieceType.King: return pieceObject.AddComponent<King>();
            default: return pieceObject.AddComponent<Pawn>();
        }
    }

    void SetupPieceVisuals(GameObject pieceObject, Card card)
    {
        SpriteRenderer spriteRenderer = pieceObject.GetComponent<SpriteRenderer>();

        // Only create sprite renderer if prefab doesn't have one
        if (spriteRenderer == null)
        {
            spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
            // Use the new GetPieceSprite function
            spriteRenderer.sprite = GetPieceSprite(card.pieceType);

            // Only apply color if using fallback sprites (not prefabs)
            Color playerColor = card.playerIndex == 0 ? Color.white : Color.black;
            spriteRenderer.color = playerColor;
        }
        // If using prefabs, don't modify the color - let the prefab handle its own appearance

        spriteRenderer.sortingOrder = 2; // Above tiles

        // Add 2D collider for interaction if not present
        if (pieceObject.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = pieceObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }

    // Get sprite from sprite array based on piece type
    public Sprite GetPieceSprite(PieceType pieceType)
    {
        // Check if sprite array is assigned and has enough elements
        if (pieceSprites == null || pieceSprites.Length == 0)
        {
            Debug.LogWarning("Piece sprites array is not assigned! Using fallback sprite creation.");
            return CreatePieceSprite(pieceType); // Fallback to programmatic sprite creation
        }
        
        // Map PieceType enum to array index
        int spriteIndex = (int)pieceType;
        
        // Validate array bounds
        if (spriteIndex >= 0 && spriteIndex < pieceSprites.Length)
        {
            // Check if the specific sprite is assigned
            if (pieceSprites[spriteIndex] != null)
            {
                return pieceSprites[spriteIndex];
            }
            else
            {
                Debug.LogWarning($"Sprite for {pieceType} at index {spriteIndex} is null! Using fallback sprite.");
            }
        }
        else
        {
            Debug.LogWarning($"Piece type {pieceType} (index {spriteIndex}) is out of bounds for sprite array (length: {pieceSprites.Length})!");
        }
        
        // Fallback to programmatic sprite creation if array access fails
        return CreatePieceSprite(pieceType);
    }

    // Create a simple sprite for piece types
    Sprite CreatePieceSprite(PieceType pieceType)
    {
        // Create a 64x64 texture with piece-specific pattern
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];

        // Simple pattern based on piece type
        Color pieceColor = GetPieceTypeColor(pieceType);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pieceColor;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    Color GetPieceTypeColor(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Pawn: return Color.green;
            case PieceType.Rook: return Color.blue;
            case PieceType.Knight: return Color.yellow;
            case PieceType.Bishop: return Color.magenta;
            case PieceType.Queen: return Color.red;
            case PieceType.King: return Color.cyan;
            default: return Color.white;
        }
    }

    void UpdateHandUI()
    {
        // Only show human player's hand information
        string handInfo = "Your hand: ";
        if (playerHands[0].Count == 0)
        {
            handInfo += "(Empty)";
        }
        else
        {
            foreach (Card card in playerHands[0])
            {
                handInfo += card.pieceType.ToString() + " ";
                if (card == selectedCard)
                    handInfo += "[SELECTED] ";
            }
        }
        Debug.Log(handInfo);

        // Computer hand is hidden - only show count for debugging
        Debug.Log($"Computer has {playerHands[1].Count} cards (hidden)");
    }

    public string GetCurrentPlayerHandString()
    {
        int currentPlayerIndex = gameManager.currentPlayer;

        // Only show details for human player
        if (currentPlayerIndex == 0)
        {
            List<Card> hand = playerHands[currentPlayerIndex];

            if (hand.Count == 0)
                return "Your hand: (Empty)";

            string handInfo = "Your hand: ";
            for (int i = 0; i < hand.Count; i++)
            {
                handInfo += $"{i + 1}. {hand[i].pieceType}";
                if (hand[i] == selectedCard)
                    handInfo += " [SELECTED]";
                if (i < hand.Count - 1)
                    handInfo += ", ";
            }

            return handInfo;
        }
        else
        {
            // Computer player - hide details
            return "Computer is holding cards (hidden)";
        }
    }

    public bool CanDrawCard(int playerIndex)
    {
        return playerHands[playerIndex].Count < maxHandSize;
    }

    public int GetHandSize(int playerIndex)
    {
        return playerHands[playerIndex].Count;
    }

    public List<Card> GetPlayerHand(int playerIndex)
    {
        return playerHands[playerIndex];
    }

    // DECK SYSTEM METHODS

    void InitializeDeck()
    {
        // Initialize separate decks for each player
        for (int playerIndex = 0; playerIndex < 2; playerIndex++)
        {
            playerDeckContents[playerIndex] = new Dictionary<PieceType, int>();

            // Set identical initial deck contents for each player
            playerDeckContents[playerIndex][PieceType.Pawn] = initialPawnCount;
            playerDeckContents[playerIndex][PieceType.Rook] = initialRookCount;
            playerDeckContents[playerIndex][PieceType.Knight] = initialKnightCount;
            playerDeckContents[playerIndex][PieceType.Bishop] = initialBishopCount;
            playerDeckContents[playerIndex][PieceType.Queen] = initialQueenCount;
            playerDeckContents[playerIndex][PieceType.King] = initialKingCount;
        }

        Debug.Log("Individual decks initialized for each player:");
        Debug.Log($"Player 1 deck: {GetDeckStatusString(0)}");
        Debug.Log($"Player 2 deck: {GetDeckStatusString(1)}");
    }
    public void TakeCardFromDeckPlayer() => TakeACardFromDeck(1);
    public bool TakeACardFromDeck(int playerIndex)
    {
        // Check if player can receive more cards
        if (playerHands[playerIndex].Count >= maxHandSize)
        {
            Debug.Log($"Player {playerIndex + 1}'s hand is full! Cannot draw from deck.");
            return false;
        }

        // Get random piece type from this player's deck
        PieceType drawnPieceType = DrawRandomPieceFromPlayerDeck(playerIndex);

        if (drawnPieceType == PieceType.Pawn && playerDeckContents[playerIndex][PieceType.Pawn] == 0)
        {
            Debug.Log($"Player {playerIndex + 1}'s deck is empty! No more cards to draw.");
            return false;
        }

        // Remove one instance from this player's deck
        playerDeckContents[playerIndex][drawnPieceType]--;

        // Create card
        Card newCard = new Card
        {
            pieceType = drawnPieceType,
            playerIndex = playerIndex,
            id = System.Guid.NewGuid().ToString()
        };

        // Add to player hand
        playerHands[playerIndex].Add(newCard);

        // Spawn visual card and animate to hand
        if (cardVisualPrefab != null)
        {
            StartCoroutine(AnimateCardToHand(newCard, playerIndex));
        }

        Debug.Log($"Player {playerIndex + 1} drew {drawnPieceType} from their personal deck! Remaining: {playerDeckContents[playerIndex][drawnPieceType]}");
        Debug.Log($"Player {playerIndex + 1} deck status: {GetDeckStatusString(playerIndex)}");

        UpdateHandUI();
        return true;
    }

    PieceType DrawRandomPieceFromPlayerDeck(int playerIndex)
    {
        // Create list of available pieces from this player's deck
        List<PieceType> availablePieces = new List<PieceType>();

        foreach (var kvp in playerDeckContents[playerIndex])
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                availablePieces.Add(kvp.Key);
            }
        }

        if (availablePieces.Count == 0)
        {
            Debug.LogWarning($"No pieces available in Player {playerIndex + 1}'s deck!");
            return PieceType.Pawn; // Fallback
        }

        // Return random piece from this player's deck
        int randomIndex = Random.Range(0, availablePieces.Count);
        return availablePieces[randomIndex];
    }

    System.Collections.IEnumerator AnimateCardToHand(Card card, int playerIndex)
    {
        Debug.Log($"🎴 AnimateCardToHand called for {card.pieceType} (Player {playerIndex + 1})");

        // Only animate cards for human player (Player 0)
        if (playerIndex != 0)
        {
            Debug.Log($"Computer drew {card.pieceType} (hidden from player)");
            yield break;
        }

        // Debug all required references
        Debug.Log($"🔍 Checking card animation references:");
        Debug.Log($"  - cardVisualPrefab: {(cardVisualPrefab != null ? "✅ Assigned" : "❌ Missing")}");
        Debug.Log($"  - deckSpawnPoint: {(deckSpawnPoint != null ? "✅ Assigned" : "❌ Missing")}");

        // Find the target slot for this card
        int cardIndex = playerHands[playerIndex].Count - 1; // Index of the newly added card
        GameObject targetSlot = GetSlot(playerIndex, cardIndex);
        
        if (targetSlot == null)
        {
            Debug.LogWarning($"❌ No slot available for card index {cardIndex}! Cannot spawn UI card.");
            yield break;
        }

        Debug.Log($"🎯 Target slot: {targetSlot.name} for card index {cardIndex}");

        // Create card visual as UI element
        GameObject cardVisual = null;
        Vector3 spawnScreenPosition = Vector3.zero;

        // Determine spawn position (deck location in screen space)
        if (deckSpawnPoint != null)
        {
            // Convert world position to screen position for UI
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                spawnScreenPosition = mainCamera.WorldToScreenPoint(deckSpawnPoint.position);
                Debug.Log($"✅ Using deck spawn point in screen space: {spawnScreenPosition}");
            }
            else
            {
                spawnScreenPosition = new Vector3(100, Screen.height - 100, 0); // Top-left of screen
                Debug.LogWarning("❌ No main camera found! Using default screen position.");
            }
        }
        else
        {
            spawnScreenPosition = new Vector3(100, Screen.height - 100, 0); // Top-left of screen
            Debug.LogWarning("❌ No deck spawn point! Using default screen position.");
        }

        // Create card visual as child of target slot
        if (cardVisualPrefab != null)
        {
            cardVisual = Instantiate(cardVisualPrefab, targetSlot.transform);
            Debug.Log($"✅ Spawned card UI prefab: {cardVisual.name} as child of {targetSlot.name}");

            // Set up UI properties
            RectTransform cardRect = cardVisual.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                // Set initial position relative to slot (start from deck position)
                Vector2 localSpawnPos = ConvertScreenToLocalPoint(targetSlot.transform as RectTransform, spawnScreenPosition);
                cardRect.anchoredPosition = localSpawnPos;
                cardRect.localScale = Vector3.one;
                Debug.Log($"✅ Set card initial local position: {localSpawnPos}");
            }

            // Try to set card data if CardVisual component exists
            CardVisual cardVisualComponent = cardVisual.GetComponent<CardVisual>();
            if (cardVisualComponent != null)
            {
                cardVisualComponent.SetCard(card);
                Debug.Log($"✅ Set card data on CardVisual component");
            }
            else
            {
                Debug.Log($"ℹ️ No CardVisual component found - using prefab as-is");
            }
        }
        else
        {
            // Create simple fallback UI card
            cardVisual = CreateSimpleUICardVisual(card, targetSlot.transform, spawnScreenPosition);
            Debug.LogWarning("❌ No card prefab assigned! Created simple fallback UI card.");
        }

        cardVisual.name = $"HandCard_{card.pieceType}_Player{playerIndex + 1}_{card.id}";

        // Store reference to card visual in Card object for later cleanup
        card.visualGameObject = cardVisual;

        // Animate card to final position (center of slot)
        RectTransform cardRectTransform = cardVisual.GetComponent<RectTransform>();
        if (cardRectTransform != null)
        {
            Debug.Log($"🎬 Starting UI animation to slot center");
            
            Vector2 startPos = cardRectTransform.anchoredPosition;
            Vector2 targetPos = Vector2.zero; // Center of the slot
            
            float journey = 0f;
            while (journey < 1f)
            {
                journey += cardMoveSpeed * Time.deltaTime;
                if (cardRectTransform != null) // Safety check
                {
                    cardRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, journey);
                }
                yield return null;
            }
            
            // Ensure final position
            if (cardRectTransform != null)
            {
                cardRectTransform.anchoredPosition = targetPos;
                Debug.Log($"✅ Card UI animation complete! Final local position: {targetPos}");
            }
        }

        Debug.Log($"💎 Card visual will remain in slot until played!");
        // DON'T destroy the card visual - keep it persistent in the slot!
    }

    GameObject CreateSimpleUICardVisual(Card card, Transform parent, Vector3 screenPosition)
    {
        // Create a simple UI card GameObject as fallback
        GameObject simpleCard = new GameObject($"SimpleUICard_{card.pieceType}");
        simpleCard.transform.SetParent(parent);
        
        // Add RectTransform for UI
        RectTransform rectTransform = simpleCard.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 120); // Card size
        rectTransform.localScale = Vector3.one;
        
        // Set initial position
        Vector2 localPos = ConvertScreenToLocalPoint(parent as RectTransform, screenPosition);
        rectTransform.anchoredPosition = localPos;

        // Add Image component with sprite from array representing the piece type
        UnityEngine.UI.Image cardImage = simpleCard.AddComponent<UnityEngine.UI.Image>();
        cardImage.sprite = GetPieceSprite(card.pieceType);
        cardImage.color = GetPieceTypeColor(card.pieceType);

        Debug.Log($"✅ Created simple UI card visual: {simpleCard.name} in slot {parent.name} at local position {localPos}");

        return simpleCard;
    }

    Vector2 ConvertScreenToLocalPoint(RectTransform parent, Vector3 screenPoint)
    {
        // Convert screen point to local point within the parent RectTransform
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out localPoint);
            return localPoint;
        }
        else if (canvas != null && canvas.worldCamera != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, canvas.worldCamera, out localPoint);
            return localPoint;
        }
        
        // Fallback - use screen coordinates directly converted to local
        Vector2 parentSize = parent.rect.size;
        return new Vector2(-parentSize.x * 0.3f, parentSize.y * 0.3f); // Offset from center
    }

    Vector3 CalculateCardPositionInHand(int playerIndex, int cardIndex)
    {
        Debug.Log($"🎯 CalculateCardPositionInHand called for Player {playerIndex + 1}, card index {cardIndex}");
        // Try to use slot position first
        Vector3 slotPosition = GetSlotWorldPosition(playerIndex, cardIndex);
        if (slotPosition != Vector3.zero)
        {
            Debug.Log($"✅ Using slot position: {slotPosition} for card index {cardIndex}");
            return slotPosition;
        }
        Debug.Log($"⚠️ No slot position found for card index {cardIndex}, trying hand areas...");

        // Fallback to hand area positioning if slots aren't available  
        if (playerHandAreas != null && cardIndex < playerHandAreas.Length && playerHandAreas[cardIndex] != null)
        {
            Debug.Log($"✅ Using hand area position for card index {cardIndex}: {playerHandAreas[cardIndex].position}");
            return playerHandAreas[cardIndex].position;
        }

        // If specific hand area not available, try spacing from first hand area
        if (playerHandAreas != null && playerHandAreas.Length > 0 && playerHandAreas[0] != null)
        {
            Transform baseHandArea = playerHandAreas[0];
            float cardSpacing = 2f; // Horizontal spacing between cards
            Vector3 spacedPosition = baseHandArea.position + Vector3.right * (cardIndex * cardSpacing);
            Debug.Log($"✅ Using spaced position from base hand area: {spacedPosition}");
            return spacedPosition;
        }

        // Ultimate fallback: default positions
        Vector3 fallbackPosition = new Vector3(cardIndex * 2f - 2f, -3f, 0);
        Debug.LogWarning($"❌ Using fallback position: {fallbackPosition}");
        return fallbackPosition;
    }

    public string GetDeckStatusString(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerDeckContents.Length)
            return "Invalid player index";

        string status = $"Player {playerIndex + 1} deck: ";
        foreach (var kvp in playerDeckContents[playerIndex])
        {
            status += $"{kvp.Key}({kvp.Value}) ";
        }
        return status;
    }

    public int GetDeckCount(PieceType pieceType, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerDeckContents.Length)
            return 0;

        return playerDeckContents[playerIndex].ContainsKey(pieceType) ? playerDeckContents[playerIndex][pieceType] : 0;
    }

    public int GetTotalDeckCount(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerDeckContents.Length)
            return 0;

        int total = 0;
        foreach (var kvp in playerDeckContents[playerIndex])
        {
            total += kvp.Value;
        }
        return total;
    }

    public bool IsDeckEmpty(int playerIndex)
    {
        return GetTotalDeckCount(playerIndex) == 0;
    }

    // PIECE VISIBILITY METHODS

    public void RevealAllPieces()
    {
        // Reveal all hidden pieces when Chess Battle phase starts
        List<ChessPiece> allPieces = boardManager.GetAllPiecesForPlayer(0);
        allPieces.AddRange(boardManager.GetAllPiecesForPlayer(1));

        foreach (ChessPiece piece in allPieces)
        {
            RevealPiece(piece.gameObject);
        }

        Debug.Log("All pieces revealed for Chess Battle phase!");
    }

    public void UpdatePieceVisibility()
    {
        // Update piece visibility based on current game phase
        List<ChessPiece> allPieces = boardManager.GetAllPiecesForPlayer(0);
        allPieces.AddRange(boardManager.GetAllPiecesForPlayer(1));

        foreach (ChessPiece piece in allPieces)
        {
            UpdateSinglePieceVisibility(piece);
        }
    }

    void UpdateSinglePieceVisibility(ChessPiece piece)
    {
        if (piece == null) return;

        bool shouldBeVisible = ShouldPieceBeVisible(piece);

        SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (shouldBeVisible)
            {
                RevealPiece(piece.gameObject);
            }
            else
            {
                HidePiece(piece.gameObject);
            }
        }
    }

    bool ShouldPieceBeVisible(ChessPiece piece)
    {
        // Always show human player pieces
        if (piece.playerIndex == 0)
            return true;

        // Only show computer pieces during Chess Battle phase  
        if (gameManager.currentPhase == GamePhase.ChessBattle)
            return true;

        // Hide computer pieces during deployment and betting phases
        return false;
    }

    void RevealPiece(GameObject pieceObject)
    {
        SpriteRenderer spriteRenderer = pieceObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Restore full opacity
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            // Remove hidden marker from name
            if (pieceObject.name.Contains("_HIDDEN"))
            {
                pieceObject.name = pieceObject.name.Replace("_HIDDEN", "");
            }
        }
    }

    void HidePiece(GameObject pieceObject)
    {
        SpriteRenderer spriteRenderer = pieceObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Make transparent
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            // Add hidden marker to name
            if (!pieceObject.name.Contains("_HIDDEN"))
            {
                pieceObject.name += "_HIDDEN";
            }
        }
    }

    // SLOT SYSTEM METHODS

    public void CreateSlotsForBoardSize(int boardSize)
    {
        // Clear existing slots first
        ClearExistingSlots();

        // Create slots equal to board width for each player
        int slotsPerPlayer = boardSize;

        // Initialize or resize playerHandAreas array to match slot count
        if (playerHandAreas == null || playerHandAreas.Length < slotsPerPlayer)
        {
            System.Array.Resize(ref playerHandAreas, slotsPerPlayer);
            Debug.Log($"🔧 Resized playerHandAreas array to {slotsPerPlayer} slots");
        }

        // Only create slots for human player (Player 0)
        CreateSlotsForPlayer(0, slotsPerPlayer);

        // Auto-assign created slots to playerHandAreas for seamless card positioning
        AssignSlotsToHandAreas(0);

        // Computer player (Player 1) gets no visible slots - their hand is hidden
        Debug.Log($"✅ Created {slotsPerPlayer} slots for Player 1 and assigned to hand areas");
        Debug.Log($"✅ Computer hand remains hidden (no slots created)");
    }

    void CreateSlotsForPlayer(int playerIndex, int slotCount)
    {
        if (slotPrefab == null)
        {
            Debug.LogWarning("Slot prefab not assigned! Cannot create slots.");
            return;
        }

        if (playerSlotParents == null || playerIndex >= playerSlotParents.Length || playerSlotParents[playerIndex] == null)
        {
            Debug.LogWarning($"Player {playerIndex + 1} slot parent not assigned!");
            return;
        }

        Transform slotParent = playerSlotParents[playerIndex];
        List<GameObject> playerSlotList = playerSlots[playerIndex];

        // Create slots for this player
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            slot.name = $"Slot_Player{playerIndex + 1}_{i + 1}";

            playerSlotList.Add(slot);
        }

        // Position slots horizontally
        PositionSlots(playerIndex);

        Debug.Log($"Created {slotCount} slots for Player {playerIndex + 1}");
    }

    void PositionSlots(int playerIndex)
    {
        List<GameObject> slots = playerSlots[playerIndex];
        if (slots.Count == 0) return;

        // Calculate total width needed
        float totalWidth = (slots.Count - 1) * slotSpacing;
        float startX = -totalWidth * 0.5f;

        // Position each slot
        for (int i = 0; i < slots.Count; i++)
        {
            RectTransform slotRect = slots[i].GetComponent<RectTransform>();
            if (slotRect != null)
            {
                Vector3 localPos = slotRect.localPosition;
                localPos.x = startX + (i * slotSpacing);
                slotRect.localPosition = localPos;
            }
        }
    }

    void AssignSlotsToHandAreas(int playerIndex)
    {
        if (playerSlots == null || playerIndex >= playerSlots.Length || playerSlots[playerIndex] == null)
        {
            Debug.LogWarning($"⚠️ No slots available for Player {playerIndex + 1}!");
            return;
        }

        List<GameObject> slots = playerSlots[playerIndex];

        // Assign each slot transform to playerHandAreas array
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerHandAreas.Length)
            {
                playerHandAreas[i] = slots[i].transform;
                Debug.Log($"📍 Assigned slot {i} ({slots[i].name}) to playerHandAreas[{i}] at position {slots[i].transform.position}");
            }
        }

        Debug.Log($"✅ Successfully assigned {slots.Count} slots to playerHandAreas for Player {playerIndex + 1}");

        // Clear any remaining hand area references beyond the slot count
        for (int i = slots.Count; i < playerHandAreas.Length; i++)
        {
            playerHandAreas[i] = null;
        }

        // Debug current hand area assignments
        LogHandAreaAssignments();
    }

    void LogHandAreaAssignments()
    {
        if (playerHandAreas == null)
        {
            Debug.Log("📋 playerHandAreas: Not initialized");
            return;
        }

        string assignments = "📋 Current playerHandAreas assignments:\n";
        for (int i = 0; i < playerHandAreas.Length; i++)
        {
            if (playerHandAreas[i] != null)
            {
                assignments += $"  [{i}] = {playerHandAreas[i].name} at {playerHandAreas[i].position}\n";
            }
            else
            {
                assignments += $"  [{i}] = null\n";
            }
        }
        Debug.Log(assignments);
    }

    void ClearExistingSlots()
    {
        for (int playerIndex = 0; playerIndex < 2; playerIndex++)
        {
            if (playerSlots[playerIndex] != null)
            {
                // Destroy existing slot GameObjects
                foreach (GameObject slot in playerSlots[playerIndex])
                {
                    if (slot != null)
                    {
                        DestroyImmediate(slot);
                    }
                }

                // Clear the list
                playerSlots[playerIndex].Clear();
            }
        }

        // Clear playerHandAreas references since slots are being destroyed
        if (playerHandAreas != null)
        {
            for (int i = 0; i < playerHandAreas.Length; i++)
            {
                playerHandAreas[i] = null;
            }
            Debug.Log("🧹 Cleared playerHandAreas references");
        }

        Debug.Log("🧹 Cleared existing slots for all players");
    }

    public int GetSlotCount(int playerIndex)
    {
        if (playerSlots == null || playerIndex < 0 || playerIndex >= playerSlots.Length)
            return 0;

        return playerSlots[playerIndex].Count;
    }

    public GameObject GetSlot(int playerIndex, int slotIndex)
    {
        if (playerSlots == null || playerIndex < 0 || playerIndex >= playerSlots.Length)
            return null;

        List<GameObject> slots = playerSlots[playerIndex];
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return null;

        return slots[slotIndex];
    }

    public Vector3 GetSlotWorldPosition(int playerIndex, int slotIndex)
    {
        GameObject slot = GetSlot(playerIndex, slotIndex);
        if (slot != null)
        {
            return slot.transform.position;
        }

        // Fallback to hand area position if slot doesn't exist
        if (playerHandAreas != null && playerIndex < playerHandAreas.Length && playerHandAreas[playerIndex] != null)
        {
            return playerHandAreas[playerIndex].position;
        }

        return Vector3.zero;
    }
}

[System.Serializable]
public class Card
{
    public string id;
    public PieceType pieceType;
    public int playerIndex;
    public CardRarity rarity;
    public Sprite CartIcon;
    [System.NonSerialized] public GameObject visualGameObject; // Reference to the visual card in hand (not serialized)
}