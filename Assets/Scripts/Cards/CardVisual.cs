using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardVisual : MonoBehaviour
{
    [Header("Card Visual Components")]
    public Image cardBackground;
    public Image pieceIcon;

    private Card cardData;

    public void SetCard(Card card)
    {
        cardData = card;
        pieceIcon.sprite = cardData.CartIcon;
    }


    // Optional: Set piece-specific sprites
    Sprite GetSpriteForPieceType(PieceType pieceType)
    {
        // Implement this method to return appropriate sprites for each piece type
        // You can load sprites from Resources or assign them in inspector
        return null;
    }

    public Card GetCard()
    {
        return cardData;
    }
}