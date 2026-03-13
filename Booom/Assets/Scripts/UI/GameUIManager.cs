using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private ScorePlayer scorePlayerPrefab;

    [SerializeField]
    private TMP_Text leaderboard;
    
    [SerializeField]
    private TMP_Text timer;

    [SerializeField]
    private TMP_Text bombType;
    
    [SerializeField]
    private TMP_Text eventPanelText;
    
    [SerializeField]
    private GameObject bombEventPanel;
    
    [SerializeField]
    private Image endGameImage;
    
    [SerializeField]
    private TMP_Text winnerText;

    [Header("Tutorial")]
    [SerializeField]
    private bool showIntroTutorial = true;

    [SerializeField]
    [TextArea(1, 3)]
    private string introTutorialText = "Capture un maximum de tuiles en evitant les pieges.";

    [SerializeField]
    private float introTutorialDuration = 3f;

    [SerializeField]
    [Range(0f, 1f)]
    private float introSoftRedAlpha = 0.22f;

    [SerializeField]
    private Color introSoftRedColor = new Color(1f, 0.35f, 0.35f, 1f);

    private readonly Dictionary<PlayerEnum, ScorePlayer> _scorePerPlayer = new ();
    
    private readonly List<KeyValuePair<PlayerEnum, ScorePlayer>> _sortedPlayerScores = new();
    private readonly List<Image> _eventPanelBackgroundImages = new();
    private readonly List<Color> _eventPanelBaseColors = new();

    private void OnDestroy()
    {
        GameManager.Instance.ScoreManager.OnScoreChanged -= RefreshScore;
    }

    private void Start()
    {
        bombEventPanel.SetActive(false);
        CacheEventPanelBackgroundImages();

        bombType.text = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();
        leaderboard.text = "Number of tiles owned";

        GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
        GameManager.Instance.StartTimer();

        if (showIntroTutorial && !string.IsNullOrWhiteSpace(introTutorialText))
        {
            StartCoroutine(ShowIntroTutorialCoroutine());
        }
    }

    public void CreateScorePlayer(PlayerEnum playerEnum)
    {
        Color c = Player.PlayerColorDict[playerEnum];
        
        var scorePlayer = Instantiate(scorePlayerPrefab, leaderboard.transform);
        scorePlayer.SetColor(c);
        scorePlayer.UpdateScore(0);
        _scorePerPlayer[playerEnum] = scorePlayer;
    
        SortLeaderboard();
    }

    private void RefreshScore(PlayerEnum player, int score)
    {
        if (!_scorePerPlayer.ContainsKey(player))
        {
            CreateScorePlayer(player);
        }
        _scorePerPlayer[player].UpdateScore(score);
        
        SortLeaderboard();
    }

    public void RefreshBombType(string newBombType)
    {
        bombType.text = newBombType;
    }
    
    private void SortLeaderboard()
    {
        _sortedPlayerScores.Clear();
        _sortedPlayerScores.AddRange(_scorePerPlayer);
        _sortedPlayerScores.Sort((p1, p2) => p2.Value.currentScore.CompareTo(p1.Value.currentScore));

        for (int i = 0; i < _sortedPlayerScores.Count; i++)
        {
            RectTransform rect = _sortedPlayerScores[i].Value.rectTransform;

            Vector2 newPos = rect.anchoredPosition;
            newPos.y = -(60f + i * 50f);
            rect.anchoredPosition = newPos;
        }
    }

    public void UpdateTimerDisplay()
    {
        timer.text = $"{GameManager.Instance.CurrentMinutes}:{GameManager.Instance.CurrentSeconds:D2}";
    }

    public void DisplayEventPanel()
    {
        eventPanelText.text = $"Bomb type is now {bombType.text}!";
        StartCoroutine(EventPanelCoroutine(3f));
    }
    
    public void DisplayEventPanel(string eventText)
    {
        eventPanelText.text = eventText;
        StartCoroutine(EventPanelCoroutine(3f));
    }

    public void EndGame()
    {
        endGameImage.gameObject.SetActive(true);
        winnerText.gameObject.SetActive(true);

        PlayerEnum winner = GameManager.Instance.ScoreManager.FindPlayerWithMostGround();
        winnerText.text = $"Player {(int)winner} won!";
        winnerText.color = Player.PlayerColorDict[winner];
    }

    private IEnumerator EventPanelCoroutine(float duration)
    {
        bombEventPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        bombEventPanel.SetActive(false);
    }

    private IEnumerator ShowIntroTutorialCoroutine()
    {
        eventPanelText.text = introTutorialText;
        ApplyIntroSoftRedBackground();

        yield return EventPanelCoroutine(introTutorialDuration);

        RestoreEventPanelBackgroundColors();
    }

    private void CacheEventPanelBackgroundImages()
    {
        _eventPanelBackgroundImages.Clear();
        _eventPanelBaseColors.Clear();

        if (bombEventPanel == null)
        {
            return;
        }

        Image[] images = bombEventPanel.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (eventPanelText != null && image.gameObject == eventPanelText.gameObject)
            {
                continue;
            }

            _eventPanelBackgroundImages.Add(image);
            _eventPanelBaseColors.Add(image.color);
        }
    }

    private void ApplyIntroSoftRedBackground()
    {
        for (int i = 0; i < _eventPanelBackgroundImages.Count; i++)
        {
            if (_eventPanelBackgroundImages[i] == null)
            {
                continue;
            }

            Color c = introSoftRedColor;
            c.a = introSoftRedAlpha;
            _eventPanelBackgroundImages[i].color = c;
        }
    }

    private void RestoreEventPanelBackgroundColors()
    {
        for (int i = 0; i < _eventPanelBackgroundImages.Count && i < _eventPanelBaseColors.Count; i++)
        {
            if (_eventPanelBackgroundImages[i] == null)
            {
                continue;
            }

            _eventPanelBackgroundImages[i].color = _eventPanelBaseColors[i];
        }
    }
}
