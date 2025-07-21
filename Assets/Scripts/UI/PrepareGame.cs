using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class BackgroundPieces
{
    public GameObject Background, Outline;
    public void SetUnActive()
    {
        if (Background != null) Background.SetActive(false);
        if (Outline != null) Outline.SetActive(false);
    }
    public void SetActive()
    {
        if (Background != null) Background.SetActive(true);
        if (Outline != null) Outline.SetActive(true);
    }
}

[System.Serializable]
public class TilePunchSettings
{
    [Header("Punch Animation Settings")]
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private Vector3 _punchScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private int _vibrato = 10;
    [SerializeField] private float _elasticity = 1f;
    [SerializeField] private Ease _easeType = Ease.OutBack;
    [SerializeField] private float _delayBetweenTiles = 0.05f;
    [SerializeField] private bool _useRandomDelay = true;
    [SerializeField] private Vector2 _randomDelayRange = new Vector2(0f, 0.1f);

    public float Duration => _duration;
    public Vector3 PunchScale => _punchScale;
    public int Vibrato => _vibrato;
    public float Elasticity => _elasticity;
    public Ease EaseType => _easeType;
    public float DelayBetweenTiles => _delayBetweenTiles;
    public bool UseRandomDelay => _useRandomDelay;
    public Vector2 RandomDelayRange => _randomDelayRange;
}

public class PrepareGame : MonoBehaviour
{
    [SerializeField]
    private string _gameSceneName = "Main Scene"; // Name of the main game scene to load
    [SerializeField] private SceneTransaction _sceneTransaction; // Reference to the SceneTransaction script for scene loading
    [SerializeField] private GameObject _blackTilePrefab, _whiteTilePrefab;
    [SerializeField] private List<BackgroundPieces> _backgrounds = new(); //3x3 => 0, 4x4 => 1, 5x5 => 2, 6x6 => 3, 7x7 => 4, 8x8 => 5
    [SerializeField] private CustomButton _3x3_Button, _4x4_Button, _5x5_Button, _6x6_Button, _7x7_Button, _8x8_Button;
    [SerializeField] private CustomButton _easy_Button, _medium_Button, _hard_Button;
    [SerializeField] private List<CustomButton> _playButtons = new();
    [SerializeField] private RectTransform _boardParent; // Assign the Board GameObject's RectTransform
    [SerializeField] private float _tileSize = 60f; // Size of each tile
    [SerializeField] private float _tileSpacing = 0f; // Optional spacing between tiles

    [Header("Tile Animation")]
    [SerializeField] private TilePunchSettings _punchSettings = new TilePunchSettings();

    private List<GameObject> _currentTiles = new List<GameObject>();

    private void OnEnable()
    {
        _3x3_Button.OnClick += () => PrepareTheGameBoard(3);
        _4x4_Button.OnClick += () => PrepareTheGameBoard(4);
        _5x5_Button.OnClick += () => PrepareTheGameBoard(5);
        _6x6_Button.OnClick += () => PrepareTheGameBoard(6);
        _7x7_Button.OnClick += () => PrepareTheGameBoard(7);
        _8x8_Button.OnClick += () => PrepareTheGameBoard(8);

        _easy_Button.OnClick += () => SaveDifficulty(0);
        _medium_Button.OnClick += () => SaveDifficulty(1);
        _hard_Button.OnClick += () => SaveDifficulty(2);

        _playButtons?.ForEach(button =>
        {
            button.OnClick += () =>
            {
                // Load the game scene when any play button is clicked
                _sceneTransaction?.LoadScene(_gameSceneName);
            };
        });
    }

    void OnDisable()
    {
        _3x3_Button.OnClick -= () => PrepareTheGameBoard(3);
        _4x4_Button.OnClick -= () => PrepareTheGameBoard(4);
        _5x5_Button.OnClick -= () => PrepareTheGameBoard(5);
        _6x6_Button.OnClick -= () => PrepareTheGameBoard(6);
        _7x7_Button.OnClick -= () => PrepareTheGameBoard(7);
        _8x8_Button.OnClick -= () => PrepareTheGameBoard(8);

        _easy_Button.OnClick -= () => SaveDifficulty(0);
        _medium_Button.OnClick -= () => SaveDifficulty(1);
        _hard_Button.OnClick -= () => SaveDifficulty(2);

        _playButtons?.ForEach(button =>
       {
           button.OnClick -= () =>
           {
               // Load the game scene when any play button is clicked
               _sceneTransaction?.LoadScene(_gameSceneName);
           };
       });
    }


    void SaveDifficulty(int difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", difficulty);
    }

    public void PrepareTheGameBoard(int size)
    {
        if (size < 3 || size > 8) return;

        // Clear existing tiles
        ClearBoard();

        // Activate correct background
        _backgrounds?.ForEach(bg => bg.SetUnActive());
        _backgrounds[size - 3].SetActive();

        PlayerPrefs.SetInt("BoardSize", size);
        PlayerPrefs.SetInt("PlayerColor", Random.Range(0, 2));

        // Create the tile grid with animation
        StartCoroutine(CreateTileGridWithAnimation(size));
    }
    private IEnumerator CreateTileGridWithAnimation(int size)
    {
        if (_boardParent == null)
        {
            Debug.LogError("Board parent not assigned! Please assign the Board GameObject's RectTransform in the inspector.");
            yield break;
        }

        if (_blackTilePrefab == null || _whiteTilePrefab == null)
        {
            Debug.LogError("Tile prefabs not assigned! Please assign black and white tile prefabs in the inspector.");
            yield break;
        }

        // Calculate total board size including spacing
        float totalTileSize = _tileSize + _tileSpacing;
        float boardSize = (size * totalTileSize) - _tileSpacing; // Remove spacing from last tile

        // Calculate starting position to center the board
        float startX = -boardSize / 2f + _tileSize / 2f;
        float startY = boardSize / 2f - _tileSize / 2f;

        // Create tiles in a grid pattern with staggered animation
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Determine tile color (checkerboard pattern)
                bool isBlackTile = (x + y) % 2 == 0;
                GameObject tilePrefab = isBlackTile ? _blackTilePrefab : _whiteTilePrefab;

                // Instantiate the tile
                GameObject newTile = Instantiate(tilePrefab, _boardParent);
                RectTransform tileRect = newTile.GetComponent<RectTransform>();

                if (tileRect != null)
                {
                    // Set up anchoring (center-based)
                    tileRect.anchorMin = new Vector2(0.5f, 0.5f);
                    tileRect.anchorMax = new Vector2(0.5f, 0.5f);
                    tileRect.pivot = new Vector2(0.5f, 0.5f);

                    // Set tile size
                    tileRect.sizeDelta = new Vector2(_tileSize, _tileSize);

                    // Calculate and set tile position
                    Vector2 tilePosition = new Vector2(
                        startX + x * totalTileSize,
                        startY - y * totalTileSize
                    );
                    tileRect.anchoredPosition = tilePosition;

                    // Start with scale 0 for punch animation
                    tileRect.localScale = Vector3.zero;

                    // Animate tile appearance with punch effect
                    AnimateTileAppearance(tileRect, x, y);
                }

                newTile.name = $"Tile_{x}_{y}";
                _currentTiles.Add(newTile);

                // Wait between tile creations if not using random delay
                if (!_punchSettings.UseRandomDelay && _punchSettings.DelayBetweenTiles > 0)
                {
                    yield return new WaitForSeconds(_punchSettings.DelayBetweenTiles);
                }
            }
        }
    }

    private void AnimateTileAppearance(RectTransform tileRect, int x, int y)
    {
        // Calculate delay for this tile
        float delay = 0f;

        if (_punchSettings.UseRandomDelay)
        {
            delay = Random.Range(_punchSettings.RandomDelayRange.x, _punchSettings.RandomDelayRange.y);
        }
        else
        {
            // Use position-based delay (diagonal wave effect)
            delay = (x + y) * _punchSettings.DelayBetweenTiles;
        }

        // Animate scale from 0 to 1 with punch effect
        tileRect.DOScale(Vector3.one, _punchSettings.Duration)
            .SetDelay(delay)
            .SetEase(_punchSettings.EaseType)
            .OnComplete(() =>
            {
                // Add punch effect after the initial scale animation
                tileRect.DOPunchScale(_punchSettings.PunchScale, _punchSettings.Duration * 0.5f,
                    _punchSettings.Vibrato, _punchSettings.Elasticity);
            });
    }


    private void ClearBoard()
    {
        // Kill any running tweens before destroying tiles
        foreach (GameObject tile in _currentTiles)
        {
            if (tile != null)
            {
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                if (tileRect != null)
                {
                    tileRect.DOKill();
                }
                DestroyImmediate(tile);
            }
        }
        _currentTiles.Clear();
    }

}