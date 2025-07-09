using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CardPrefabSetup : MonoBehaviour
{
    [Header("Auto Setup Card Prefab")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private Vector2 cardSize = new Vector2(100, 140);
    
    void Awake()
    {
        if (autoSetup)
        {
            SetupCardPrefab();
        }
    }
    
    [ContextMenu("Setup Card Prefab")]
    public void SetupCardPrefab()
    {
        // Get or add RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        rectTransform.sizeDelta = cardSize;
        
        // Get or add main background Image
        Image cardBackground = GetComponent<Image>();
        if (cardBackground == null)
        {
            cardBackground = gameObject.AddComponent<Image>();
        }
        cardBackground.color = Color.white;
        cardBackground.raycastTarget = true;
        
        // Get or add CardVisual component
        CardVisual cardVisual = GetComponent<CardVisual>();
        if (cardVisual == null)
        {
            cardVisual = gameObject.AddComponent<CardVisual>();
        }
        
        // Setup child components
        SetupPieceIcon(cardVisual);
        SetupPieceText(cardVisual);
        SetupSelectionHighlight(cardVisual);
        
        Debug.Log($"✅ Card prefab setup complete for {gameObject.name}");
    }
    
    void SetupPieceIcon(CardVisual cardVisual)
    {
        Transform iconTransform = transform.Find("PieceIcon");
        GameObject iconObject;
        
        if (iconTransform == null)
        {
            iconObject = new GameObject("PieceIcon");
            iconObject.transform.SetParent(transform);
        }
        else
        {
            iconObject = iconTransform.gameObject;
        }
        
        // Setup RectTransform
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        if (iconRect == null)
        {
            iconRect = iconObject.AddComponent<RectTransform>();
        }
        
        iconRect.anchorMin = new Vector2(0.5f, 0.6f);
        iconRect.anchorMax = new Vector2(0.5f, 0.6f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(60, 60);
        
        // Setup Image
        Image iconImage = iconObject.GetComponent<Image>();
        if (iconImage == null)
        {
            iconImage = iconObject.AddComponent<Image>();
        }
        iconImage.raycastTarget = false;
        
        // Assign to CardVisual
        cardVisual.pieceIcon = iconImage;
    }
    
    void SetupPieceText(CardVisual cardVisual)
    {
        Transform textTransform = transform.Find("PieceText");
        GameObject textObject;
        
        if (textTransform == null)
        {
            textObject = new GameObject("PieceText");
            textObject.transform.SetParent(transform);
        }
        else
        {
            textObject = textTransform.gameObject;
        }
        
        // Setup RectTransform
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        if (textRect == null)
        {
            textRect = textObject.AddComponent<RectTransform>();
        }
        
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 0.3f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Setup TextMeshPro
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = textObject.AddComponent<TextMeshProUGUI>();
        }
        
        textComponent.text = "Piece";
        textComponent.fontSize = 12;
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.raycastTarget = false;
        
        // Assign to CardVisual
        cardVisual.pieceNameText = textComponent;
    }
    
    void SetupSelectionHighlight(CardVisual cardVisual)
    {
        Transform highlightTransform = transform.Find("SelectionHighlight");
        GameObject highlightObject;
        
        if (highlightTransform == null)
        {
            highlightObject = new GameObject("SelectionHighlight");
            highlightObject.transform.SetParent(transform);
        }
        else
        {
            highlightObject = highlightTransform.gameObject;
        }
        
        // Setup RectTransform
        RectTransform highlightRect = highlightObject.GetComponent<RectTransform>();
        if (highlightRect == null)
        {
            highlightRect = highlightObject.AddComponent<RectTransform>();
        }
        
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.anchoredPosition = Vector2.zero;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;
        
        // Setup Image for highlight border
        Image highlightImage = highlightObject.GetComponent<Image>();
        if (highlightImage == null)
        {
            highlightImage = highlightObject.AddComponent<Image>();
        }
        
        highlightImage.color = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
        highlightImage.raycastTarget = false;
        
        // Start disabled
        highlightObject.SetActive(false);
        
        // Assign to CardVisual
        cardVisual.selectionHighlight = highlightObject;
        cardVisual.cardBackground = GetComponent<Image>();
    }
}