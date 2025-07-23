using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WinLoseUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI loseText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private Ease animationEase = Ease.OutBounce;
    
    void Start()
    {
        // Initialize panels as hidden
        if (winPanel != null) 
        {
            winPanel.SetActive(false);
            winPanel.transform.localScale = Vector3.zero;
        }
        if (losePanel != null) 
        {
            losePanel.SetActive(false);
            losePanel.transform.localScale = Vector3.zero;
        }
        
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }
    
    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.DOScale(Vector3.one, animationDuration).SetEase(animationEase);
            
            if (winText != null)
            {
                winText.text = "Congratulations!\nYou Won!";
                winText.color = Color.green;
            }
        }
        
        Debug.Log("Player Wins!");
    }
    
    public void ShowLose()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            losePanel.transform.DOScale(Vector3.one, animationDuration).SetEase(animationEase);
            
            if (loseText != null)
            {
                loseText.text = "Game Over!\nYou Lost!";
                loseText.color = Color.red;
            }
        }
        
        Debug.Log("Player Loses!");
    }
    
    public void RestartGame()
    {
        // Add fade out animation before restarting
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            });
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}