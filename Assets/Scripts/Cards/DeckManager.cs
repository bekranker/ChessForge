using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour, IInteractable
{
    [Header("Deck Manager Settings")]
    [SerializeField] private float _cardInitDuration;
    [SerializeField] private Transform _deckParent;
    [SerializeField] private List<Transform> _cards = new();
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Vector2 _deckDirection;
    public List<PieceCard> Deck = new();
    public List<PieceCard> Hand { get; private set; } = new();
    [SerializeField] private Transform _handPoint;
    [SerializeField] private ChessBoard _chessBoard;
    [SerializeField] private int _baseSortingOrder = 0;
    [SerializeField] private float _angleStep;
    [SerializeField] private float _radius;
    [SerializeField] private float _curveHeight;

    private bool _clickable;
    public void RearrangeHand()
    {
        Hand.RemoveAll(item => item == null);
        int count = Hand.Count;
        float radius = _radius;
        float angleStep = _angleStep;
        float curveHeight = _curveHeight;
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

        // Update sorting orders after rearranging
        UpdateAllCardsSortingOrder();
    }

    private void UpdateAllCardsSortingOrder()
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            Hand[i].SetSortingOrder(i);
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
    private IEnumerator SpawnAllCards()
    {
        for (int i = 0; i < 16; i++)
        {
            GameObject spawnedCard = Instantiate(_cardPrefab, _deckParent);
            spawnedCard.GetComponent<SpriteRenderer>().sortingOrder = i;
            Vector3 targetPosition = new Vector3(i * 0.1f * _deckDirection.x, i * 0.1f * _deckDirection.y, 0);
            spawnedCard.transform.DOLocalMove(targetPosition, _cardInitDuration).SetEase(Ease.OutBack);
            _cards.Add(spawnedCard.transform);
            yield return new WaitForSeconds(_cardInitDuration / 15f);
        }
    }
    public PieceCard DrawCard()
    {
        if (Deck.Count > 0)
        {
            PieceCard selectedCard = Deck[Random.Range(0, Deck.Count)];
            RemoveCard(selectedCard);
            _cards[0].gameObject.SetActive(false);
            _cards.RemoveAt(0);
            return selectedCard;
        }
        return null;
    }

    public bool SpawnHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PieceCard card = DrawCard();
            if (card == null) return false;

            PieceCard spawnedCard = Instantiate(card, transform.position, Quaternion.identity);
            spawnedCard.Initialize(card.Data, _chessBoard, this);
            Hand.Add(spawnedCard);

            // Set initial sorting order for the newly spawned card
            SpriteRenderer cardRenderer = spawnedCard.GetComponent<SpriteRenderer>();
            if (cardRenderer != null)
            {
                cardRenderer.sortingOrder = _baseSortingOrder + Hand.Count - 1;
            }
        }
        RearrangeHand(); // Hepsini yerleştir ve sorting order'ları güncelle
        return true;
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
        if (!_clickable) return false;
        return SpawnHand(1);
    }

    public IEnumerator InitDeck()
    {
        int deckSize = PlayerPrefs.GetInt("BoardSize", 8);
        yield return StartCoroutine(SpawnAllCards());
        _clickable = true;
    }
}