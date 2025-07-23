using DG.Tweening;
using UnityEngine;

public class BetPiece : MonoBehaviour, IInteractable
{
    [SerializeField] private float betAmount = 10f;
    [SerializeField] private LayerMask _pieceLayerMask;
    [SerializeField] private BetPiece _coinPrefab;
    [SerializeField] private Vector3 _startPos;
    public void CanceledInteraction()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).OnComplete(() => Destroy(gameObject));
    }


    public void InteractDown()
    {
        BetPiece newCoin = Instantiate(_coinPrefab, _startPos, Quaternion.identity);
    }

    public bool InteractUP()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, 100, _pieceLayerMask);
        if (hit2D.collider != null)
        {
            print("Hit: " + hit2D.collider.name);
            if (hit2D.collider.TryGetComponent(out ChessPiece piece))
            {
                print("sa");
            }
        }
        return false;
    }
}