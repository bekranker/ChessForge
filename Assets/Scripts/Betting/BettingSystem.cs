using UnityEngine;
using System.Collections.Generic;

public class BettingSystem : MonoBehaviour
{
    [Header("Betting State")]
    public int[] playerTotalBets = new int[2];
    public int[] playerRemainingCoins = new int[2];
    public bool[] playerBettingComplete = new bool[2];
    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI[] remainingCoinsText = new TMPro.TextMeshProUGUI[2];
    public TMPro.TextMeshProUGUI bettingInstructionsText;
    
    private GameManager gameManager;
    private BoardManager boardManager;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        boardManager = manager.boardManager;
        
        // Ensure arrays are properly initialized
        if (playerTotalBets == null || playerTotalBets.Length != 2)
        {
            playerTotalBets = new int[2];
        }
        
        if (playerRemainingCoins == null || playerRemainingCoins.Length != 2)
        {
            playerRemainingCoins = new int[2];
        }
        
        if (playerBettingComplete == null || playerBettingComplete.Length != 2)
        {
            playerBettingComplete = new bool[2];
        }
        
        Debug.Log("BettingSystem initialized successfully with proper array sizes.");
    }
    
    public void StartBettingPhase()
    {
        // Initialize betting amounts based on board size
        int totalBetPerPlayer = gameManager.GetGameConfig().GetTotalBetForBoardSize(gameManager.selectedBoardSize);
        
        for (int i = 0; i < 2; i++)
        {
            playerTotalBets[i] = totalBetPerPlayer;
            playerRemainingCoins[i] = totalBetPerPlayer;
            playerBettingComplete[i] = false;
        }
        
        Debug.Log($"Betting phase started. Each player has {totalBetPerPlayer} coins to distribute.");
        UpdateBettingUI();
    }
    
    public void PlaceBetOnPiece(ChessPiece piece, int betAmount)
    {
        int playerIndex = piece.playerIndex;
        
        // Validate bet
        if (playerBettingComplete[playerIndex])
        {
            Debug.Log($"Player {playerIndex + 1} has already completed betting!");
            return;
        }
        
        if (betAmount > playerRemainingCoins[playerIndex])
        {
            Debug.Log($"Not enough coins! Player {playerIndex + 1} has {playerRemainingCoins[playerIndex]} remaining.");
            return;
        }
        
        if (betAmount < 0)
        {
            Debug.Log("Bet amount must be positive!");
            return;
        }
        
        // Place the bet
        int previousBet = piece.GetCoinsOnPiece();
        piece.SetCoinsOnPiece(betAmount);
        
        // Update remaining coins
        playerRemainingCoins[playerIndex] += previousBet - betAmount;
        
        Debug.Log($"Player {playerIndex + 1} bet {betAmount} coins on {piece.pieceType} at {piece.boardPosition}");
        UpdateBettingUI();
    }
    
    public void AutoDistributeRemainingCoins(int playerIndex)
    {
        if (playerBettingComplete[playerIndex])
            return;
            
        List<ChessPiece> playerPieces = boardManager.GetAllPiecesForPlayer(playerIndex);
        int remainingCoins = playerRemainingCoins[playerIndex];
        
        if (playerPieces.Count == 0 || remainingCoins <= 0)
        {
            CompleteBettingForPlayer(playerIndex);
            return;
        }
        
        // Distribute coins evenly
        int coinsPerPiece = remainingCoins / playerPieces.Count;
        int extraCoins = remainingCoins % playerPieces.Count;
        
        for (int i = 0; i < playerPieces.Count; i++)
        {
            ChessPiece piece = playerPieces[i];
            int additionalCoins = coinsPerPiece + (i < extraCoins ? 1 : 0);
            int newTotal = piece.GetCoinsOnPiece() + additionalCoins;
            piece.SetCoinsOnPiece(newTotal);
        }
        
        playerRemainingCoins[playerIndex] = 0;
        CompleteBettingForPlayer(playerIndex);
        
        Debug.Log($"Auto-distributed {remainingCoins} remaining coins for Player {playerIndex + 1}");
    }
    
    public void CompleteBettingForPlayer(int playerIndex)
    {
        playerBettingComplete[playerIndex] = true;
        Debug.Log($"Player {playerIndex + 1} completed betting.");
        UpdateBettingUI();
    }
    
    public bool BothPlayersCompletedBetting()
    {
        return playerBettingComplete[0] && playerBettingComplete[1];
    }
    
    public void CalculateGameRewards(int winnerPlayer)
    {
        Debug.Log($"Calculating game rewards. Winner: {winnerPlayer}");
        
        if (winnerPlayer == -1)
        {
            // Draw - everyone gets their bets back
            Debug.Log("Draw: All players get their bets back.");
            return;
        }
        
        // Safety check for valid player index
        if (winnerPlayer < 0 || winnerPlayer >= playerTotalBets.Length)
        {
            Debug.LogError($"Invalid winner player index: {winnerPlayer}. Must be between 0 and {playerTotalBets.Length - 1}.");
            return;
        }
        
        int loserPlayer = 1 - winnerPlayer;
        
        // Safety check for loser player index
        if (loserPlayer < 0 || loserPlayer >= playerTotalBets.Length)
        {
            Debug.LogError($"Invalid loser player index: {loserPlayer}. Must be between 0 and {playerTotalBets.Length - 1}.");
            return;
        }
        
        // Calculate winner's rewards
        int winnerTotalBet = playerTotalBets[winnerPlayer];
        int loserTotalBet = playerTotalBets[loserPlayer];
        int survivingPieceBonus = CalculateSurvivingPieceBonus(winnerPlayer);
        
        int totalReward = winnerTotalBet + loserTotalBet + survivingPieceBonus;
        
        Debug.Log($"Player {winnerPlayer + 1} wins!");
        Debug.Log($"- Reclaimed bet: {winnerTotalBet} coins");
        Debug.Log($"- Won from opponent: {loserTotalBet} coins");
        Debug.Log($"- Surviving piece bonus: {survivingPieceBonus} coins");
        Debug.Log($"- Total reward: {totalReward} coins");
        
        // TODO: Add to player's actual coin balance
    }
    
    int CalculateSurvivingPieceBonus(int playerIndex)
    {
        // Safety check for valid player index
        if (playerIndex < 0 || playerIndex > 1)
        {
            Debug.LogError($"Invalid player index in CalculateSurvivingPieceBonus: {playerIndex}");
            return 0;
        }
        
        if (boardManager == null)
        {
            Debug.LogError("BoardManager is null in CalculateSurvivingPieceBonus");
            return 0;
        }
        
        List<ChessPiece> survivingPieces = boardManager.GetAllPiecesForPlayer(playerIndex);
        int bonus = 0;
        
        foreach (ChessPiece piece in survivingPieces)
        {
            if (piece != null)
            {
                bonus += piece.GetCoinsOnPiece();
            }
        }
        
        return bonus;
    }
    
    void UpdateBettingUI()
    {
        for (int i = 0; i < 2; i++)
        {
            if (remainingCoinsText[i] != null)
            {
                string status = playerBettingComplete[i] ? " (Complete)" : "";
                remainingCoinsText[i].text = $"Player {i + 1}: {playerRemainingCoins[i]} coins{status}";
            }
        }
        
        if (bettingInstructionsText != null)
        {
            if (BothPlayersCompletedBetting())
            {
                bettingInstructionsText.text = "All betting complete! Prepare for battle!";
            }
            else
            {
                bettingInstructionsText.text = $"Player {gameManager.currentPlayer + 1}: Distribute your coins among your pieces";
            }
        }
    }
    
    // UI Helper methods for betting interface
    public void OnBetButtonClicked(ChessPiece piece, int amount)
    {
        PlaceBetOnPiece(piece, amount);
    }
    
    public void OnCompleteBettingClicked()
    {
        int currentPlayer = gameManager.currentPlayer;
        
        Debug.Log($"OnCompleteBettingClicked called for Player {currentPlayer + 1}");
        
        // Auto-distribute any remaining coins
        if (playerRemainingCoins[currentPlayer] > 0)
        {
            Debug.Log($"Auto-distributing {playerRemainingCoins[currentPlayer]} remaining coins for Player {currentPlayer + 1}");
            AutoDistributeRemainingCoins(currentPlayer);
        }
        else
        {
            Debug.Log($"Player {currentPlayer + 1} has no remaining coins, marking betting complete");
            CompleteBettingForPlayer(currentPlayer);
        }
        
        // Check if both players completed betting
        if (BothPlayersCompletedBetting())
        {
            Debug.Log("Both players completed betting, transitioning to Chess Battle phase");
            gameManager.StartChessBattlePhase();
        }
        else
        {
            Debug.Log("Advancing to next player's betting turn");
            gameManager.NextTurn();
        }
    }
    
    public int GetRemainingCoins(int playerIndex)
    {
        return playerRemainingCoins[playerIndex];
    }
    
    public int GetTotalBet(int playerIndex)
    {
        return playerTotalBets[playerIndex];
    }
    
    public bool IsPlayerBettingComplete(int playerIndex)
    {
        return playerBettingComplete[playerIndex];
    }
}