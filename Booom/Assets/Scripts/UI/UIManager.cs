using System;
using System.Collections;
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
    private TMP_Text bombType;
    
    [SerializeField]
    private TMP_Text eventPanelText;
    
    [SerializeField]
    private GameObject bombEventPanel;
    
    [SerializeField]
    public Image endGameImage;

    private readonly Dictionary<PlayerEnum, ScorePlayer> _scorePerPlayer = new ();
    private string _statTracked = "Eliminations";
    
    private readonly List<KeyValuePair<PlayerEnum, ScorePlayer>> _sortedPlayerScores = new();

    private void OnEnable()
    {
        GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
    }

    private void OnDisable()
    {
        GameManager.Instance.ScoreManager.OnScoreChanged -= RefreshScore;
        if (PlayerInputManager.instance != null)
            PlayerInputManager.instance.onPlayerJoined -= AddPlayer;
    }

    private void Start()
    {
        bombEventPanel.SetActive(false);

        if (GameManager.Instance.IsSpreadingMode)
            _statTracked = "Number of tiles owned";

        bombType.text = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();
        leaderboard.text = _statTracked;
        PlayerInputManager.instance.onPlayerJoined += AddPlayer;

        GameManager.Instance.StartTimer();
    }

    public void AddPlayer(PlayerInput playerInput)
    {
        if (playerInput != null)
        {
            switch (playerInput.playerIndex)
            {
                case 0:
                    Player.PlayerColorDict[PlayerEnum.Player1] = Color.red;
                    break;
                case 1:
                    Player.PlayerColorDict[PlayerEnum.Player2] = Color.green;
                    break;
                case 2:
                    Player.PlayerColorDict[PlayerEnum.Player3] = Color.blue;
                    break;
                case 3:
                    Player.PlayerColorDict[PlayerEnum.Player4] = Color.yellow;
                    break;
                default:
                    throw new Exception("Player Input Manager tried to create invalid Player");
            }
        }
        else
        {
            throw new Exception("There's no active player input");
        }
        
        PlayerEnum playerEnum = (PlayerEnum) playerInput.playerIndex + 1;
        Color c = Player.PlayerColorDict[playerEnum];
        
        
        if (!_scorePerPlayer.ContainsKey(playerEnum))
        {
            var scorePlayer = Instantiate(scorePlayerPrefab, leaderboard.transform);
            scorePlayer.SetColor(c);
            scorePlayer.UpdateScore(0);
            _scorePerPlayer[playerEnum] = scorePlayer;
            
            SortLeaderboard();
        }
    }

    private void RefreshScore(PlayerEnum player, int score)
    {
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

    public void DisplayEventPanel(string bombTypeName)
    {
        eventPanelText.text = $"Bomb type is now {bombTypeName}!";
        StartCoroutine(EventPanelCoroutine());
    }

    private IEnumerator EventPanelCoroutine()
    {
        bombEventPanel.SetActive(true);
        yield return new WaitForSeconds(3); // to tweak
        bombEventPanel.SetActive(false);
    }
}
