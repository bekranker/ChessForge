using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
public class PieceCard : MonoBehaviour, IInteractable
{
    [Header("Board Tile Props")]
    [SerializeField] private LayerMask _tileLayerMask;

    [Header("Piece Card Components")]
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<TMP_Text> _names = new();
    [SerializeField] private TMP_Text _betTMP;
    [SerializeField] private List<Image> _symbols = new();

    [Header("Piece Card Data")]
    [SerializeField] private PieceCardSO _data;

    public PieceCardSO Data => _data;
    private ChessBoard _chessBoard;
    private DeckManager _deckManager;
    public float BetCounter;
    public bool Putted;
    public void SetBet(float bet)
    {
        BetCounter += bet;
        _betTMP.text = BetCounter.ToString("F2");
    }
    public void Initialize(PieceCardSO data, ChessBoard chessBoard, DeckManager deckManager)
    {
        _deckManager = deckManager;
        _data = data;
        if (PlayerPrefs.GetInt("PlayerIndex") == 0)
            _spriteRenderer.sprite = data.IconWhite;
        else
            _spriteRenderer.sprite = data.IconBlack;
        _chessBoard = chessBoard;
        foreach (var name in _names)
        {
            name.text = data.Name;
        }
        foreach (var symbol in _symbols)
        {
            if (PlayerPrefs.GetInt("PlayerIndex") == 0)
                symbol.sprite = data.IconWhite;
            else
                symbol.sprite = data.IconBlack;
        }
    }
    private List<TileConfig> _availableTiles = new List<TileConfig>();
    public void InteractDown()
    {
        _availableTiles = _chessBoard.GetAvailablePlacementTiles();
    }

    public bool InteractUP()
    {
        _chessBoard.ClearAvailablePlacementTiles();
        if (Putted) return false;
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, _tileLayerMask);
        if (hit.collider != null)
        {
            //call piece Spawn function
            TileConfig tileConfig = _chessBoard.FindTile(hit.collider.gameObject);

            if (!_availableTiles.Contains(tileConfig))
            {
                Debug.Log("Tile not available for placement: " + tileConfig.Position);
                return false;
            }

            _deckManager.RemoveCard(this);

            if (tileConfig.Occupied)
            {
                return false;
            }

            ChessPiece piece = Instantiate(_data.PiecePrefab, transform.position, Quaternion.identity);

            piece.InitializePiece(this, tileConfig.Position, (PlayerColors)(PlayerPrefs.GetInt("PlayerIndex")));

            tileConfig.SetTile(piece);

            TakeCardAsUsed();
            
            // Notify game manager that player has completed their action
            ChessGameManager gameManager = FindFirstObjectByType<ChessGameManager>();
            if (gameManager != null)
            {
                gameManager.OnPlayerAction();
            }
            
            return true;
        }
        return false;
    }
    private void TakeCardAsUsed()
    {
        _deckManager.RearrangeHand();
        _deckManager.RemoveCard(this);
        Putted = true;
        _spriteRenderer.DOFade(0.5f, 0.5f);
    }
    public void CanceledInteraction()
    {
        _deckManager.AddCard(this);
        // Handle any cleanup or state reset if needed
        Debug.Log("Interaction canceled for PieceCard: " + _data.Name);
    }
    public void SetSortingOrder(int order)
    {
        _sortingGroup.sortingOrder = order;
        _canvas.sortingOrder = order; // Canvas should be above the sprite
    }
}