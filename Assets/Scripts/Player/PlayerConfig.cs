using UnityEngine;
using System.Collections.Generic;

public class PlayerConfig : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private string _playerName = "Player";
    [SerializeField] private int _playerScore = 0;
    [SerializeField] private PlayerColors _playerColor;

    public string PlayerName => _playerName;
    public int PlayerScore => _playerScore;
    public PlayerColors PlayerColor => _playerColor;

    public static PlayerConfig Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        SetPlayerColor();
    }
    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    public void SetPlayerScore(int score)
    {
        _playerScore = score;
    }

    public void SetPlayerColor()
    {
        int index = PlayerPrefs.GetInt("PlayerColor", 0);
        _playerColor = (PlayerColors)index;
    }
}