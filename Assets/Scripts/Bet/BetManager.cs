using TMPro;
using UnityEngine;

public class BetManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _betText;
    private float _betAmount, _currentBet;

    public void Start()
    {
        _betAmount = PlayerPrefs.GetInt("BoardSize") * 100;
        _currentBet = _betAmount;
        SetBetText(_betAmount);
    }

    public void SetBetText(float betAmount)
    {
        if (_betText != null)
        {
            _betText.text = betAmount.ToString();
        }
        else
        {
            Debug.LogWarning("Bet text is not assigned in the BetManager.");
        }
    }
    public bool TakeBet(float betAmount)
    {
        if (betAmount <= _currentBet)
        {
            _currentBet -= betAmount;
            SetBetText(_currentBet);
            return true;
        }
        else
        {
            Debug.LogWarning("Insufficient bet amount.");
            return false;
        }
    }
}