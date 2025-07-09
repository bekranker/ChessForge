using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ChessForge/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Board Configuration")]
    public BoardSizeConfig[] boardConfigs;
    
    [Header("Card Rarity Configuration")]
    public CardRarityConfig[] cardRarities;
    
    [Header("General Settings")]
    public int maxHandSize = 3;
    public float turnTimeLimit = 30f;
    public int drawRepeatsLimit = 3;
    
    public void Initialize()
    {
        if (boardConfigs == null || boardConfigs.Length == 0)
        {
            InitializeDefaultBoardConfigs();
        }
        
        if (cardRarities == null || cardRarities.Length == 0)
        {
            InitializeDefaultCardRarities();
        }
    }
    
    void InitializeDefaultBoardConfigs()
    {
        boardConfigs = new BoardSizeConfig[]
        {
            new BoardSizeConfig { boardSize = BoardSize.Size3x3, playerSideRows = 2, piecesPerPlayer = 6, turns = 3, totalBet = 300 },
            new BoardSizeConfig { boardSize = BoardSize.Size4x4, playerSideRows = 2, piecesPerPlayer = 8, turns = 4, totalBet = 400 },
            new BoardSizeConfig { boardSize = BoardSize.Size5x5, playerSideRows = 3, piecesPerPlayer = 10, turns = 5, totalBet = 500 },
            new BoardSizeConfig { boardSize = BoardSize.Size6x6, playerSideRows = 3, piecesPerPlayer = 12, turns = 6, totalBet = 600 },
            new BoardSizeConfig { boardSize = BoardSize.Size7x7, playerSideRows = 4, piecesPerPlayer = 14, turns = 7, totalBet = 700 },
            new BoardSizeConfig { boardSize = BoardSize.Size8x8, playerSideRows = 4, piecesPerPlayer = 16, turns = 8, totalBet = 800 }
        };
    }
    
    void InitializeDefaultCardRarities()
    {
        cardRarities = new CardRarityConfig[]
        {
            new CardRarityConfig { pieceType = PieceType.Pawn, rarity = CardRarity.Common, weight = 40 },
            new CardRarityConfig { pieceType = PieceType.Knight, rarity = CardRarity.Medium, weight = 15 },
            new CardRarityConfig { pieceType = PieceType.Bishop, rarity = CardRarity.Medium, weight = 15 },
            new CardRarityConfig { pieceType = PieceType.King, rarity = CardRarity.Medium, weight = 15 },
            new CardRarityConfig { pieceType = PieceType.Rook, rarity = CardRarity.Rare, weight = 10 },
            new CardRarityConfig { pieceType = PieceType.Queen, rarity = CardRarity.VeryRare, weight = 5 }
        };
    }
    
    public BoardSizeConfig GetBoardConfig(BoardSize size)
    {
        foreach (var config in boardConfigs)
        {
            if (config.boardSize == size)
                return config;
        }
        return boardConfigs[0]; // Default fallback
    }
    
    public int GetTurnsForBoardSize(BoardSize size)
    {
        return GetBoardConfig(size).turns;
    }
    
    public int GetTotalBetForBoardSize(BoardSize size)
    {
        return GetBoardConfig(size).totalBet;
    }
    
    public int GetPlayerSideRowsForBoardSize(BoardSize size)
    {
        return GetBoardConfig(size).playerSideRows;
    }
    
    public int GetPiecesPerPlayerForBoardSize(BoardSize size)
    {
        return GetBoardConfig(size).piecesPerPlayer;
    }
    
    public PieceType DrawRandomCard()
    {
        int totalWeight = 0;
        foreach (var rarity in cardRarities)
        {
            totalWeight += rarity.weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var rarity in cardRarities)
        {
            currentWeight += rarity.weight;
            if (randomValue < currentWeight)
            {
                return rarity.pieceType;
            }
        }
        
        return PieceType.Pawn; // Fallback
    }
}

[System.Serializable]
public class BoardSizeConfig
{
    public BoardSize boardSize;
    public int playerSideRows;
    public int piecesPerPlayer;
    public int turns;
    public int totalBet;
}

[System.Serializable]
public class CardRarityConfig
{
    public PieceType pieceType;
    public CardRarity rarity;
    public int weight;
}

public enum CardRarity
{
    Common,
    Medium,
    Rare,
    VeryRare
}