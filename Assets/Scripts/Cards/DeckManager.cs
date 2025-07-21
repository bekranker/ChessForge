using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour, IInteractable
{
    public List<PieceCard> Deck = new();
    public List<PieceCard> Hand { get; private set; } = new();

    [SerializeField] private Transform _handPoint;
    [SerializeField] private ChessBoard _chessBoard;
    public void RearrangeHand()
    {
        Hand.RemoveAll(item => item == null);
        int count = Hand.Count;
        float radius = 3f;
        float angleStep = 15f;
        float curveHeight = 1.5f;
        Vector3 center = _handPoint.position;

        float startAngle = -angleStep * (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            PieceCard card = Hand[i];

            float angle = startAngle + i * angleStep;
            float radians = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(radians) * radius;
            float z = Mathf.Cos(radians) * radius;
            float y = center.y + Mathf.Abs(Mathf.Cos(radians)) * curveHeight;

            Vector3 targetPos = new Vector3(center.x + x, y, center.z + z);

            card.transform.DOLocalMove(targetPos, 0.5f);
            card.transform.DOLocalRotate(new Vector3(0, 0, -angle), 0.5f);
            card.transform.SetParent(_handPoint); // Kartı el noktasına ata
            card.GetComponent<CardTiltEffect2D>().originalRotation = Quaternion.Euler(0, 0, -angle); // Kartın orijinal rotasını ayarla
        }
    }

    public void AddCard(PieceCard card)
    {
        Deck.Add(card);
        RearrangeHand(); // Kart eklendikten sonra el düzenlemesini güncelle
    }

    public void RemoveCard(PieceCard card)
    {
        if (Deck.Contains(card))
        {
            Deck.Remove(card);
        }
        else
        {
            Debug.LogWarning("Card not found in the deck.");
        }
    }

    public PieceCard DrawCard()
    {
        if (Deck.Count > 0)
        {
            PieceCard selectedCard = Deck[Random.Range(0, Deck.Count)];
            RemoveCard(selectedCard);
            return selectedCard;
        }
        return null;
    }

    public void SpawnHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PieceCard card = DrawCard();
            if (card == null) continue;

            PieceCard spawnedCard = Instantiate(card, transform.position, Quaternion.identity);
            spawnedCard.Initialize(card.Data, _chessBoard, this);
            Hand.Add(spawnedCard);
        }

        RearrangeHand(); // Hepsini yerleştir
    }
    public void InteractDown()
    {
    }
    public void CanceledInteraction()
    {
        // Handle any cleanup or state reset if needed
        Debug.Log("Interaction canceled for DeckManager.");
    }
    public bool InteractUP()
    {
        SpawnHand(1); // Her etkileşimde 1 kart çek
        return true;
    }
}
