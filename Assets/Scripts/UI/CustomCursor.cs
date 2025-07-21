using DG.Tweening;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _animationPunchDuration;
    [SerializeField] private float _harmonicMovementDuration;
    [SerializeField] private SpriteRenderer _cursorRenderer;
    public static CustomCursor Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane; // Set the distance from the camera
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _speed); // Adjust the speed as needed
    }
    public void HoveredCursor()
    {
        DOTween.Kill(_cursorRenderer.transform);

        _cursorRenderer.transform.DOLocalMoveY(.6f, _animationDuration).SetEase(Ease.Linear);
        HoverHarmonicAnimation();
    }
    private void HoverHarmonicAnimation()
    {
        _cursorRenderer.transform.DORotate(new Vector3(0, 0, 15), _harmonicMovementDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _cursorRenderer.transform.DORotate(new Vector3(0, 0, -15), _harmonicMovementDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                HoverHarmonicAnimation(); // Call HoveredCursor again to create a loop
            });
        });
    }
    public void HoldedCursor()
    {
        DOTween.Kill(_cursorRenderer.transform);
        _cursorRenderer.transform.DORotate(Vector3.zero, _animationPunchDuration).SetEase(Ease.Linear);
        _cursorRenderer.transform.DOLocalMoveY(0, _animationPunchDuration).SetEase(Ease.Linear);
    }
    public void UnHoveredCursor()
    {
        DOTween.Kill(_cursorRenderer.transform);
        _cursorRenderer.transform.DORotate(Vector3.zero, _animationDuration).SetEase(Ease.Linear);
        _cursorRenderer.transform.DOLocalMoveY(0, _animationDuration).SetEase(Ease.Linear);
    }
}
