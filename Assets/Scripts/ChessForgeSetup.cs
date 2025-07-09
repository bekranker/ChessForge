using UnityEngine;

public class ChessForgeSetup : MonoBehaviour
{
    [Header("Setup ChessForge Game")]
    [Tooltip("Click to automatically setup the ChessForge game")]
    public bool setupGame = false;
    
    void OnValidate()
    {
        if (setupGame)
        {
            setupGame = false;
            SetupChessForgeGame();
        }
    }
    
    [ContextMenu("Setup ChessForge Game")]
    public void SetupChessForgeGame()
    {
        SetupCamera();
        SetupGameManager();
        
        Debug.Log("ChessForge: Drafted Tactics setup completed! Press Play to start the game.");
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        
        // Configure camera for 2D
        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 6;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        
        // Ensure camera is set to render 2D layers
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }
    
    void SetupGameManager()
    {
        GameManager existingGame = FindObjectOfType<GameManager>();
        if (existingGame != null)
        {
            DestroyImmediate(existingGame.gameObject);
        }
        
        GameObject gameManagerObject = new GameObject("ChessForge Game Manager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();
        
        // Create and setup all required components
        SetupBoardManager(gameManagerObject, gameManager);
        SetupCardSystem(gameManagerObject, gameManager);
        SetupBettingSystem(gameManagerObject, gameManager);
        SetupChessCombat(gameManagerObject, gameManager);
        SetupPlayerManager(gameManagerObject, gameManager);
        SetupUIManager(gameManagerObject, gameManager);
        
        // Create UI Canvas
        SetupUI(gameManagerObject);
    }
    
    void SetupBoardManager(GameObject parent, GameManager gameManager)
    {
        GameObject boardObject = new GameObject("Board Manager");
        boardObject.transform.SetParent(parent.transform);
        BoardManager boardManager = boardObject.AddComponent<BoardManager>();
        gameManager.boardManager = boardManager;
    }
    
    void SetupCardSystem(GameObject parent, GameManager gameManager)
    {
        GameObject cardObject = new GameObject("Card System");
        cardObject.transform.SetParent(parent.transform);
        CardSystem cardSystem = cardObject.AddComponent<CardSystem>();
        gameManager.cardSystem = cardSystem;
    }
    
    void SetupBettingSystem(GameObject parent, GameManager gameManager)
    {
        GameObject bettingObject = new GameObject("Betting System");
        bettingObject.transform.SetParent(parent.transform);
        BettingSystem bettingSystem = bettingObject.AddComponent<BettingSystem>();
        gameManager.bettingSystem = bettingSystem;
    }
    
    void SetupChessCombat(GameObject parent, GameManager gameManager)
    {
        GameObject combatObject = new GameObject("Chess Combat");
        combatObject.transform.SetParent(parent.transform);
        ChessCombat chessCombat = combatObject.AddComponent<ChessCombat>();
        gameManager.chessCombat = chessCombat;
    }
    
    void SetupPlayerManager(GameObject parent, GameManager gameManager)
    {
        GameObject playerObject = new GameObject("Player Manager");
        playerObject.transform.SetParent(parent.transform);
        PlayerManager playerManager = playerObject.AddComponent<PlayerManager>();
        gameManager.playerManager = playerManager;
    }
    
    // In ChessForgeSetup.cs, replace the SetupUIManager method:
    void SetupUIManager(GameObject parent, GameManager gameManager)
    {
        GameObject uiObject = new GameObject("UI Manager");
        uiObject.transform.SetParent(parent.transform);
        SimpleUIManager uiManager = uiObject.AddComponent<SimpleUIManager>();
        gameManager.uiManager = uiManager;
    }

    
    void SetupUI(GameObject gameManager)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create basic UI elements
        CreateStatusUI(canvas.transform, gameManager);
    }
    
    void CreateStatusUI(Transform canvasTransform, GameObject gameManager)
    {
        // Phase Text
        GameObject phaseTextObject = new GameObject("Phase Text");
        phaseTextObject.transform.SetParent(canvasTransform);
        
        TMPro.TextMeshProUGUI phaseText = phaseTextObject.AddComponent<TMPro.TextMeshProUGUI>();
        phaseText.text = "Phase: Setup";
        phaseText.fontSize = 24;
        phaseText.color = Color.white;
        phaseText.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform phaseRect = phaseText.GetComponent<RectTransform>();
        phaseRect.anchorMin = new Vector2(0.5f, 1f);
        phaseRect.anchorMax = new Vector2(0.5f, 1f);
        phaseRect.anchoredPosition = new Vector2(0, -30);
        phaseRect.sizeDelta = new Vector2(400, 50);
        
        // Player Text
        GameObject playerTextObject = new GameObject("Player Text");
        playerTextObject.transform.SetParent(canvasTransform);
        
        TMPro.TextMeshProUGUI playerText = playerTextObject.AddComponent<TMPro.TextMeshProUGUI>();
        playerText.text = "Current Player: 1";
        playerText.fontSize = 20;
        playerText.color = Color.white;
        playerText.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform playerRect = playerText.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0.5f, 1f);
        playerRect.anchorMax = new Vector2(0.5f, 1f);
        playerRect.anchoredPosition = new Vector2(0, -70);
        playerRect.sizeDelta = new Vector2(300, 40);
        
        // Timer Text
        GameObject timerTextObject = new GameObject("Timer Text");
        timerTextObject.transform.SetParent(canvasTransform);
        
        TMPro.TextMeshProUGUI timerText = timerTextObject.AddComponent<TMPro.TextMeshProUGUI>();
        timerText.text = "Time: 30s";
        timerText.fontSize = 18;
        timerText.color = Color.yellow;
        timerText.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform timerRect = timerText.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0, -110);
        timerRect.sizeDelta = new Vector2(200, 35);
        
        // Assign to GameManager
        GameManager gm = gameManager.GetComponent<GameManager>();
        gm.phaseText = phaseText;
        gm.playerText = playerText;
        gm.timerText = timerText;
    }
}