using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int totalCoins;
    public int gamesPlayed;
    public int gamesWon;
    public int gamesLost;
    public int gamesDrawn;
    public int level;
    public int experience;
    
    public PlayerData(string name)
    {
        playerName = name;
        totalCoins = 1000; // Starting coins
        gamesPlayed = 0;
        gamesWon = 0;
        gamesLost = 0;
        gamesDrawn = 0;
        level = 1;
        experience = 0;
    }
    
    public float GetWinRate()
    {
        return gamesPlayed > 0 ? (float)gamesWon / gamesPlayed : 0f;
    }
}

public class PlayerManager : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerData[] players = new PlayerData[2];
    
    [Header("Experience System")]
    public int baseExpPerLevel = 100;
    public int expPerWin = 50;
    public int expPerLoss = 20;
    public int expPerDraw = 30;
    
    private GameManager gameManager;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        
        // Initialize default players for Player vs Computer
        if (players[0] == null) players[0] = new PlayerData("Player");
        if (players[1] == null) players[1] = new PlayerData("Computer");
        
        LoadPlayerData();
    }
    
    public void AddCoins(int playerIndex, int amount)
    {
        if (playerIndex >= 0 && playerIndex < 2)
        {
            players[playerIndex].totalCoins += amount;
            Debug.Log($"{players[playerIndex].playerName} gained {amount} coins. Total: {players[playerIndex].totalCoins}");
        }
    }
    
    public bool SpendCoins(int playerIndex, int amount)
    {
        if (playerIndex >= 0 && playerIndex < 2)
        {
            if (players[playerIndex].totalCoins >= amount)
            {
                players[playerIndex].totalCoins -= amount;
                Debug.Log($"{players[playerIndex].playerName} spent {amount} coins. Remaining: {players[playerIndex].totalCoins}");
                return true;
            }
        }
        return false;
    }
    
    public void RecordGameResult(int winnerPlayer)
    {
        players[0].gamesPlayed++;
        players[1].gamesPlayed++;
        
        if (winnerPlayer == -1) // Draw
        {
            players[0].gamesDrawn++;
            players[1].gamesDrawn++;
            AddExperience(0, expPerDraw);
            AddExperience(1, expPerDraw);
            Debug.Log("Game ended in a draw. Both players gain experience.");
        }
        else
        {
            int loserPlayer = 1 - winnerPlayer;
            players[winnerPlayer].gamesWon++;
            players[loserPlayer].gamesLost++;
            
            AddExperience(winnerPlayer, expPerWin);
            AddExperience(loserPlayer, expPerLoss);
            
            Debug.Log($"{players[winnerPlayer].playerName} wins! Winner gains {expPerWin} exp, loser gains {expPerLoss} exp.");
        }
        
        SavePlayerData();
    }
    
    void AddExperience(int playerIndex, int exp)
    {
        players[playerIndex].experience += exp;
        
        // Check for level up
        int expNeeded = GetExpNeededForLevel(players[playerIndex].level + 1);
        if (players[playerIndex].experience >= expNeeded)
        {
            LevelUp(playerIndex);
        }
    }
    
    void LevelUp(int playerIndex)
    {
        players[playerIndex].level++;
        
        // Give bonus coins for leveling up
        int bonusCoins = players[playerIndex].level * 50;
        AddCoins(playerIndex, bonusCoins);
        
        Debug.Log($"{players[playerIndex].playerName} leveled up to Level {players[playerIndex].level}! Gained {bonusCoins} bonus coins!");
    }
    
    int GetExpNeededForLevel(int level)
    {
        return baseExpPerLevel * level;
    }
    
    public PlayerData GetPlayerData(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < 2)
            return players[playerIndex];
        return null;
    }
    
    public bool HasEnoughCoinsForGame(int playerIndex, int requiredCoins)
    {
        return players[playerIndex].totalCoins >= requiredCoins;
    }
    
    void LoadPlayerData()
    {
        // Load from PlayerPrefs (simple persistence)
        for (int i = 0; i < 2; i++)
        {
            string prefix = $"Player{i}_";
            
            if (PlayerPrefs.HasKey(prefix + "Name"))
            {
                players[i].playerName = PlayerPrefs.GetString(prefix + "Name", $"Player {i + 1}");
                players[i].totalCoins = PlayerPrefs.GetInt(prefix + "Coins", 1000);
                players[i].gamesPlayed = PlayerPrefs.GetInt(prefix + "GamesPlayed", 0);
                players[i].gamesWon = PlayerPrefs.GetInt(prefix + "GamesWon", 0);
                players[i].gamesLost = PlayerPrefs.GetInt(prefix + "GamesLost", 0);
                players[i].gamesDrawn = PlayerPrefs.GetInt(prefix + "GamesDrawn", 0);
                players[i].level = PlayerPrefs.GetInt(prefix + "Level", 1);
                players[i].experience = PlayerPrefs.GetInt(prefix + "Experience", 0);
            }
        }
        
        Debug.Log("Player data loaded.");
    }
    
    void SavePlayerData()
    {
        for (int i = 0; i < 2; i++)
        {
            string prefix = $"Player{i}_";
            
            PlayerPrefs.SetString(prefix + "Name", players[i].playerName);
            PlayerPrefs.SetInt(prefix + "Coins", players[i].totalCoins);
            PlayerPrefs.SetInt(prefix + "GamesPlayed", players[i].gamesPlayed);
            PlayerPrefs.SetInt(prefix + "GamesWon", players[i].gamesWon);
            PlayerPrefs.SetInt(prefix + "GamesLost", players[i].gamesLost);
            PlayerPrefs.SetInt(prefix + "GamesDrawn", players[i].gamesDrawn);
            PlayerPrefs.SetInt(prefix + "Level", players[i].level);
            PlayerPrefs.SetInt(prefix + "Experience", players[i].experience);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Player data saved.");
    }
    
    public void ResetPlayerData()
    {
        for (int i = 0; i < 2; i++)
        {
            players[i] = new PlayerData($"Player {i + 1}");
        }
        SavePlayerData();
        Debug.Log("Player data reset to defaults.");
    }
}