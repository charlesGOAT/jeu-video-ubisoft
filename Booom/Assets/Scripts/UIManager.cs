using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private ScorePlayer scorePlayerPrefab;

    [SerializeField]
    private TMP_Text leaderboard;
    
    [SerializeField]
    private TMP_Text timer;

    [SerializeField]
    public Image endGameImage;

    private readonly Dictionary<PlayerEnum, ScorePlayer> _scorePerPlayer = new ();
    private string _statTracked = "Eliminations";
    
    private float _timeRemaining;
    private bool _timerRunning;
    
    private readonly List<KeyValuePair<PlayerEnum, ScorePlayer>> _sortedPlayerScores = new();

    private void OnEnable()
    {
        GameManager.Instance.ScoreManager.OnScoreChanged += Refresh;
    }

    private void OnDisable()
    {
        GameManager.Instance.ScoreManager.OnScoreChanged -= Refresh;
    }

    private void Start()
    {
        if (GameManager.Instance.isSpreadingMode)
            _statTracked = "Number of tiles owned";
        
        leaderboard.text = _statTracked;

        StartTimer();
    }

    private void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            UpdateTimerDisplay();
            GameManager.Instance.EndGame();
            return;
        }

        UpdateTimerDisplay();
    }

    public void PlayerAdded(PlayerEnum playerEnum, Color c)
    {
        if (!_scorePerPlayer.ContainsKey(playerEnum))
        {
            var scorePlayer = Instantiate(scorePlayerPrefab, leaderboard.transform);
            scorePlayer.SetColor(c);
            scorePlayer.UpdateScore(0);
            _scorePerPlayer[playerEnum] = scorePlayer;
            
            SortLeaderboard();
        }
    }

    private void Refresh(PlayerEnum player, int score)
    {
        _scorePerPlayer[player].UpdateScore(score);
        
        SortLeaderboard();
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

    private void StartTimer()
    {
        _timeRemaining = GameConstants.GAME_DURATION;
        _timerRunning = true;
        UpdateTimerDisplay();
    }
    
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        timer.text = $"{minutes}:{seconds:D2}";
    }
}