using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class InfoPanelController : MonoBehaviour
{
    [Header("Info Panel Settings")]
    [SerializeField] private string _defaultText;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private float _textDelay = 0.05f;

    public static InfoPanelController Instance { get; private set; }

    private Coroutine _currentWriteCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        WriteText(_defaultText);
    }

    public void WriteText(string text)
    {
        StopWrite();
        _currentWriteCoroutine = StartCoroutine(WriteTextIE(text));
    }

    public void StopWrite()
    {
        if (_currentWriteCoroutine != null)
        {
            StopCoroutine(_currentWriteCoroutine);
            _currentWriteCoroutine = null;
        }
        _infoText.text = "";
    }
    public void WriteDefaultText()
    {
        WriteText(_defaultText);
    }
    private IEnumerator WriteTextIE(string text)
    {
        _infoText.text = "";
        foreach (char c in text)
        {
            _infoText.text += c;
            yield return new WaitForSeconds(_textDelay);
        }
        _currentWriteCoroutine = null;
    }
}
