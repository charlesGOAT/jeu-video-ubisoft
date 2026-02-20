using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private ScorePlayer scorePlayerPrefab;

    [SerializeField]
    private TMP_Text leaderboard;

    private Dictionary<PlayerEnum, ScorePlayer> _scorePerPlayer = new ();
    private bool _spreadMode = false;
    private string _statTracked = "Eliminations";

    private void OnEnable()
    {
         //GameManager.Instance.ScoreManager.OnScoreChanged += Refresh;
         //PlayerInputManager.instance.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        // GameManager.Instance.ScoreManager.OnScoreChanged -= Refresh;
        PlayerInputManager.instance.onPlayerJoined -= AddPlayer;
    }

    private void Start()
    {
        _spreadMode = GameManager.Instance.isSpreadingMode;
        
        if (_spreadMode)
            _statTracked = "Number of tiles owned";
        
        leaderboard.text = _statTracked;
        PlayerInputManager.instance.onPlayerJoined += AddPlayer;
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
                    Player.PlayerColorDict[PlayerEnum.Player2] = Color.yellow;
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
                
            RectTransform textTrans = scorePlayer.GetComponentInChildren<RectTransform>();
            Vector2 newPos = textTrans.anchoredPosition;
            newPos.y = - (60f + (int)(playerEnum - 1) * 50f);
            Debug.Log(newPos.y);
                
            textTrans.anchoredPosition = newPos;
                
            scorePlayer.UpdateScore(0);
            _scorePerPlayer[playerEnum] = scorePlayer;
        }
    }

    private void Refresh(PlayerEnum player, int score)
    {
        _scorePerPlayer[player].UpdateScore(score);
    }
}