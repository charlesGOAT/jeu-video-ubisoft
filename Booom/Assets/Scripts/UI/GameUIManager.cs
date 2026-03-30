using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text eventPanelText;
    
    [SerializeField]
    private GameObject bombEventPanel;
    
    [SerializeField] 
    private Image vinylImage;
    
    [SerializeField]
    private TMP_Text tutoText;

    [SerializeField] 
    private List<TMP_Text> playerPercents;
    
    private string _bombType = "";
    private readonly HashSet<PlayerEnum> _playersTutoDone = new();
    private bool _tutoEnded;
  
    private Material _vinylMaterial;
    private Animator _vinylAnimator;
    private bool _hasVinylAnimationStarted = false;
    
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

        _bombType = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();

        _vinylMaterial = vinylImage.material;
        _vinylAnimator = vinylImage.GetComponentInParent<Animator>();
        _vinylMaterial.SetFloat("_Speed", GameManager.Instance.GameDuration);

        if (SceneManager.GetActiveScene().name != "Tuto")
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
            GameManager.Instance.StartTimer();
            InitializeScorePlayers();
        }
        else
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += CheckEndTuto;
        }
    }

    private void InitializeScorePlayers()
    {
        foreach (var player in Player.ActivePlayers)
        {
            playerPercents[(int)player.PlayerNb - 1].transform.parent.gameObject.SetActive(true);
        }
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
        playerPercents[(int)player - 1].text = $"{score}%";
    }

    public void RefreshBombType(string newBombType)
    {
        _bombType = newBombType;
    }

    public void UpdateTimerDisplay()
    {
        var gameManager = GameManager.Instance;
        _vinylMaterial.SetFloat("_Timey", gameManager.GameDuration - gameManager.TimeRemaining);

        if (!_hasVinylAnimationStarted && gameManager.TimeRemaining <= 30)
        {
            _vinylAnimator.SetBool("lastStretch", true);
            _hasVinylAnimationStarted = true;
        }
    }

    public void DisplayEventPanel()
    {
        eventPanelText.text = $"Bomb type is now {_bombType}!";
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
