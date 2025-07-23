using DG.Tweening;
using UnityEngine;

public class BetPiece : MonoBehaviour, IInteractable
{
    [SerializeField] private float betAmount = 10f;
    [SerializeField] private LayerMask _pieceLayerMask;
    [SerializeField] private BetPiece _coinPrefab;
    [SerializeField] private Vector3 _startPos;

    [SerializeField] public BetManager BetManager;


    public void CanceledInteraction()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).OnComplete(() => Destroy(gameObject));
    }


    public void InteractDown()
    {
        if (!BetManager.TakeBet(betAmount)) return;
        BetPiece newCoin = Instantiate(_coinPrefab, _startPos, Quaternion.identity);
        newCoin.BetManager = BetManager;
    }

    public bool InteractUP()
    {
        if (!BetManager.TakeBet(betAmount)) return false;

        RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, 100, _pieceLayerMask);
        if (hit2D.collider != null)
        {
            if (hit2D.collider.TryGetComponent(out ChessPiece piece))
            {
                // Check if this piece belongs to the current player
                ChessGameManager gameManager = FindFirstObjectByType<ChessGameManager>();
                if (gameManager != null && piece.pieceColor == gameManager.currentPlayer)
                {
                    piece.PieceCard.SetBet(betAmount);

                    // Visual feedback
                    transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
                    Destroy(gameObject);
                    return true;
                }
            }
        }
        return false;
    }
}