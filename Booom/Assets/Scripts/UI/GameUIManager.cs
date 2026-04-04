using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct Objects
{
    public GameObject[] objects;
}

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private LocalizeStringEvent eventPanelTextLocalize;
    
    [SerializeField]
    private GameObject bombEventPanel;
    
    [SerializeField] 
    private Image vinylImage;

    [SerializeField] 
    private List<TMP_Text> playerPercents;

    [SerializeField] 
    private List<Objects> cube;
    
    [SerializeField] 
    private List<Objects> fire;

    [SerializeField] 
    private TextMeshProUGUI countdownText;
    
    [SerializeField] 
    private LocalizeStringEvent countdownTextLocalized;

    private string _bombType = "";

    private Material _vinylMaterial;
    private Animator _vinylAnimator;
    private bool _hasVinylAnimationStarted = false;
    
    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name != "Tuto")
            GameManager.Instance.ScoreManager.OnScoreChanged -= RefreshScore;
    }

    private void Start()
    {
        bombEventPanel.SetActive(false);

        _bombType = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();

        _vinylMaterial = vinylImage.material;
        _vinylAnimator = vinylImage.GetComponentInParent<Animator>();
        _vinylMaterial.SetFloat("_Speed", GameManager.Instance.GameDuration);
        _vinylMaterial.SetFloat("_Timey", -2);

        if (SceneManager.GetActiveScene().name != "Tuto")
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
            InitializeScorePlayers();
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        int second = 0;
        while (second < 3)
        {
            countdownText.text = $"{3 - second}";
            SoundManager.Instance.OnGameStarted(second++);
            yield return new WaitForSecondsRealtime(1f);
        }
        countdownTextLocalized.RefreshString();
        // todo : make text shake?
        SoundManager.Instance.PlayBattleMucic();
        GameManager.Instance.StartTimer();

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void InitializeScorePlayers()
    {
        foreach(PlayerEnum player in Player.ActivePlayers.Keys)
        {
            playerPercents[(int)player - 1].transform.parent.gameObject.SetActive(true);
            RefreshScore(player, 0);
        }
    }

    public void NewKillStreak(in PlayerEnum player, int killStreakLevel)
    {
        cube[(int)player - 1].objects[killStreakLevel - 1].SetActive(true);
        fire[(int)player - 1].objects[killStreakLevel - 1].SetActive(true);
    }
    
    private void RefreshScore(PlayerEnum player, int score)
    {
        int percent = (int)(((float)score / GameManager.Instance.GridManager.CapturableTilesCount) * 100);

        playerPercents[(int)player - 1].text = $"{percent}%";
    }

    public void RefreshBombType(string newBombType)
    {
        _bombType = newBombType;
    }

    public void UpdateTimerDisplay()
    {
        var gameManager = GameManager.Instance;
        _vinylMaterial.SetFloat("_Timey", gameManager.GameDuration - gameManager.TimeRemaining);

        if (!_hasVinylAnimationStarted && gameManager.TimeRemaining <= 15)
        {
            _vinylAnimator.SetBool("lastStretch", true);
            _hasVinylAnimationStarted = true;
        }
    }

    public void DisplayEventPanel()
    {
        LocalizedString locString = new LocalizedString
        {
            TableReference = "UI_Text",
            TableEntryReference = "BombEvent"
        };
        LocalizedString bombType = new LocalizedString
        {
            TableReference = "UI_Text",
            TableEntryReference = _bombType.Replace(" ", "")
        };
        locString.Arguments = new object[] { bombType.GetLocalizedString() };
        
        eventPanelTextLocalize.StringReference = locString;
        eventPanelTextLocalize.RefreshString();
        
        StartCoroutine(EventPanelCoroutine());
    }
    
    public void DisplayEventPanel(LocalizedString eventText)
    {
        eventPanelTextLocalize.StringReference = eventText;
        eventPanelTextLocalize.RefreshString();
        StartCoroutine(EventPanelCoroutine());
    }

    private IEnumerator EventPanelCoroutine()
    {
        bombEventPanel.SetActive(true);
        yield return new WaitForSeconds(3); // to tweak
        bombEventPanel.SetActive(false);
    }
}
