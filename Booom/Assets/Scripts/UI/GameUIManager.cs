using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    
    [SerializeField]
    private TMP_Text tutoText;

    private readonly HashSet<PlayerEnum> _playersTutoDone = new();
    
    private readonly Dictionary<PlayerEnum, ScorePlayer> _scorePerPlayer = new ();
    private readonly List<KeyValuePair<PlayerEnum, ScorePlayer>> _sortedPlayerScores = new();

    private bool _tutoEnded;

    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name != "Tuto")
            GameManager.Instance.ScoreManager.OnScoreChanged -= RefreshScore;
        else
            GameManager.Instance.ScoreManager.OnScoreChanged -= CheckEndTuto;
    }

    private void Start()
    {
        bombEventPanel.SetActive(false);

        bombType.text = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();
        leaderboard.text = "Number of tiles owned";

        if (SceneManager.GetActiveScene().name != "Tuto")
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
            GameManager.Instance.StartTimer();
        }
        else
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += CheckEndTuto;
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

    private void CheckEndTuto(PlayerEnum player, int score)
    {
        if (player == PlayerEnum.None || score <= 1) return;
        
        _playersTutoDone.Add(player);

        if (_playersTutoDone.Count == LobbyManager.JoinedPlayers.Count && !_tutoEnded)
        {
            _tutoEnded = true;
            StartCoroutine(EndTutoCoroutine());
        }
    }
    
    private IEnumerator EndTutoCoroutine()
    {
        int countdown = 5;

        while (countdown > 0)
        {
            tutoText.text = $"GET READY TO FIGHT IN {countdown}...";
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        tutoText.text = "FIGHT!";
        yield return new WaitForSeconds(1f);

        GameManager.Instance.NewRound();

        SceneManager.LoadScene(RoundManager.FindNextMap());
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
        StartCoroutine(EventPanelCoroutine());
    }
    
    public void DisplayEventPanel(string eventText)
    {
        eventPanelText.text = eventText;
        StartCoroutine(EventPanelCoroutine());
    }

    private IEnumerator EventPanelCoroutine()
    {
        bombEventPanel.SetActive(true);
        yield return new WaitForSeconds(3); // to tweak
        bombEventPanel.SetActive(false);
    }
}
