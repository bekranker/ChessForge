using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PieceCard : MonoBehaviour, IInteractable
{
    [Header("Board Tile Props")]
    [SerializeField] private LayerMask _tileLayerMask;

    [Header("Piece Card Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<TMP_Text> _names = new();
    [SerializeField] private List<Image> _symbols = new();

    [Header("Piece Card Data")]
    [SerializeField] private PieceCardSO _data;

    public PieceCardSO Data => _data;
    private ChessBoard _chessBoard;
    private DeckManager _deckManager;

    public void Initialize(PieceCardSO data, ChessBoard chessBoard, DeckManager deckManager)
    {
        _deckManager = deckManager;
        _data = data;
        _spriteRenderer.sprite = data.Icon;
        _chessBoard = chessBoard;
        foreach (var name in _names)
        {
            name.text = data.Name;
        }
        foreach (var symbol in _symbols)
        {
            symbol.sprite = data.Icon;
        }
    }

    public void InteractDown()
    {
    }

    public bool InteractUP()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, _tileLayerMask);
        if (hit.collider != null)
        {
            //call piece Spawn function
            TileConfig tileConfig = _chessBoard.FindTile(hit.collider.gameObject);
            _deckManager.RemoveCard(this);
            if (tileConfig.Occupied) return false;
            ChessPiece piece = Instantiate(PlayerConfig.Instance.PlayerColor == PlayerColors.White ? _data.WhitePiecePrefab : _data.BlackPiecePrefab, transform.position, Quaternion.identity);
            tileConfig.SetTile(piece);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
    public void CanceledInteraction()
    {
        _deckManager.AddCard(this);
        // Handle any cleanup or state reset if needed
        Debug.Log("Interaction canceled for PieceCard: " + _data.Name);
    }
}