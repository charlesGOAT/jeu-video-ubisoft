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
    
    private string _bombType = "";

    private Material _vinylMaterial;
    private Animator _vinylAnimator;
    private bool _hasVinylAnimationStarted = false;
    
    private void OnDestroy()
    {
        String sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Tuto" &&  sceneName != "Menu")
            GameManager.Instance.ScoreManager.OnScoreChanged -= RefreshScore;
    }

    private void Start()
    {
        bombEventPanel.SetActive(false);

        _bombType = GameManager.Instance.EventManager.CurrentBombType.ToString().AddSpacesBeforeCaps();

        _vinylMaterial = vinylImage.material;
        _vinylAnimator = vinylImage.GetComponentInParent<Animator>();
        _vinylMaterial.SetFloat("_Speed", GameManager.Instance.GameDuration);

        String sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Tuto" &&  sceneName != "Menu")
        {
            GameManager.Instance.ScoreManager.OnScoreChanged += RefreshScore;
            StartCoroutine(Countdown());
            InitializeScorePlayers();
        }
    }

    private IEnumerator Countdown()
    {
        int second = 0;
        while (second < 4)
        {
            SoundManager.Instance.OnGameStarted(second++);
            // display text
            yield return new WaitForSecondsRealtime(1f);
        }
        SoundManager.Instance.PlayBattleMucic();
        GameManager.Instance.StartTimer();
    }

    private void InitializeScorePlayers()
    {
        foreach(PlayerEnum player in Player.ActivePlayers.Keys)
        {
            playerPercents[(int)player - 1].transform.parent.gameObject.SetActive(true);
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
