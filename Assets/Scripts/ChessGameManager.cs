using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ChessGameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private ChessBoard _chessBoard;
    [SerializeField] private DeckManager _deckManager;

    [Header("Game States Props")]
    [SerializeField] private float _betTime;
    [SerializeField] private float _playTime;

    private float _timerCouter;

    [Header("Game State")]
    public bool gameActive = true;
    public PlayerColors currentPlayer = PlayerColors.White;
    public GamePhases GamePhase = GamePhases.Setup;

    IEnumerator Start()
    {
        yield return StartCoroutine(StartPhase());
        InitTimer();

    }
    public void NextPhase()
    {
        switch (GamePhase)
        {
            case GamePhases.Setup:
                GamePhase = GamePhases.Betting;
                break;
            case GamePhases.Betting:
                GamePhase = GamePhases.Playing;
                break;
            case GamePhases.Playing:
                GamePhase = GamePhases.Ended;
                break;
            case GamePhases.Ended:
                gameActive = false;
                break;
        }
    }
    public void NextPlayer()
    {
        currentPlayer = currentPlayer == PlayerColors.White ? PlayerColors.Black : PlayerColors.White;
    }
    private IEnumerator StartPhase()
    {
        yield return StartCoroutine(_deckManager.InitDeck());
    }
    private void InitTimer()
    {
        if (GamePhase == GamePhases.Betting)
        {
            _timerCouter = _betTime;
        }
        else if (GamePhase == GamePhases.Playing)
        {
            _timerCouter = _playTime;
        }
    }
}