using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using TMPro;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Color _normalTextColor, _hoveredTextColor, _clickTextColor;
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Sprite _normalSprite, _hoverSprite, _clickSprite;
    [SerializeField] private float _scaleDuration = 1.1f;

    private Vector3 _startScale;
    public event Action OnClick, OnHover, OnExit;
    public bool Interactable;
    private bool _entered, _pointerDown;

    void Start()
    {
        _startScale = transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable) return;
        if (_buttonText != null)
            _buttonText.color = _hoveredTextColor;
        _entered = true;
        if (_hoverSprite != null)
            _buttonImage.sprite = _hoverSprite;
        OnHover?.Invoke();
        CustomCursor.Instance.HoveredCursor();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactable) return;
        if (_buttonText != null)
            _buttonText.color = _normalTextColor;
        if (_normalSprite != null)
            _buttonImage.sprite = _normalSprite;
        DOTween.Kill(transform);
        transform.DOScale(_startScale, _scaleDuration).SetEase(Ease.Linear);
        OnExit?.Invoke();
        CustomCursor.Instance.UnHoveredCursor();
        _entered = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable) return;
        DOTween.Kill(transform);
        CustomCursor.Instance.HoldedCursor();
        transform.localScale = _startScale;
        transform.DOScale(Vector3.one * .75f, _scaleDuration).SetEase(Ease.Linear);
        _pointerDown = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_pointerDown) return;
        if (!_entered) return;
        if (!Interactable) return;
        CustomCursor.Instance.HoveredCursor();
        if (_buttonText != null)
            _buttonText.color = _hoveredTextColor;
        if (_hoverSprite != null)
            _buttonImage.sprite = _hoverSprite;
        DOTween.Kill(transform);
        transform.DOScale(_startScale, _scaleDuration).SetEase(Ease.Linear);
        OnClick?.Invoke();
        _pointerDown = false;
    }
}