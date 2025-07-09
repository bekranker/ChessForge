using UnityEngine;

public class BoardTile : MonoBehaviour
{
    public int x, y;
    public BoardManager boardManager;
    
    void Start()
    {
        // Add 2D collider for mouse interaction (remove convex property for 2D)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }
    
    public void Initialize(int x, int y, BoardManager manager)
    {
        this.x = x;
        this.y = y;
        this.boardManager = manager;
    }
    
    void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            Vector2Int position = new Vector2Int(x, y);
            
            switch (GameManager.Instance.currentPhase)
            {
                case GamePhase.CardDeployment:
                    HandleCardDeploymentClick(position);
                    break;
                case GamePhase.ChessBattle:
                    HandleChessBattleClick(position);
                    break;
            }
        }
    }
    
    void HandleCardDeploymentClick(Vector2Int position)
    {
        // Get the card system and try to place selected card
        CardSystem cardSystem = FindObjectOfType<CardSystem>();
        if (cardSystem != null)
        {
            cardSystem.TryPlaceCardAt(position);
        }
    }
    
    void HandleChessBattleClick(Vector2Int position)
    {
        // Get the chess combat system and handle piece selection/movement
        ChessCombat chessCombat = FindObjectOfType<ChessCombat>();
        if (chessCombat != null)
        {
            chessCombat.HandleTileClick(position);
        }
    }
}