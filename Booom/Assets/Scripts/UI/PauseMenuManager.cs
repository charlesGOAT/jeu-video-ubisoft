using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class PauseMenuManager : MonoBehaviour
{
    private bool _isMenuActivated;
    private PlayerEnum _playerInControl = PlayerEnum.None;
    
    [SerializeField]
    private VideoPlayer vPlayer;

    [SerializeField] 
    private Animator maskAnimator;
    
    [SerializeField] 
    private Canvas canvas;

    private PlayerInput _currentInput;
    private InputSystemUIInputModule[] _uiInputs;

    [SerializeField] 
    private GameObject continueButton;
    
    [SerializeField] 
    private GameObject skipTuto;
    
    private RawImage _targetImage;
    private Coroutine _coroutine;

    private TutoUIManager _tutoUIManager;

    private void Awake()
    {
        //vPlayer.Prepare();
        _targetImage = vPlayer.gameObject.GetComponent<RawImage>();
    }

    private void Start()
    {
        canvas.worldCamera = Camera.main;
        _uiInputs = FindObjectsByType<InputSystemUIInputModule>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (SceneManager.GetActiveScene().name.Equals("Tuto"))
        {
            skipTuto.SetActive(true);
            _tutoUIManager = FindAnyObjectByType<TutoUIManager>();
        }
    }
    
    public void TogglePauseMenu(in PlayerEnum playerNb, PlayerInput playerInput)
    {
        if (_playerInControl != PlayerEnum.None)
        {
            if (_playerInControl != playerNb) return;
            
            ToggleOffMenu();
            _playerInControl = PlayerEnum.None;
        }
        else
        {
            _currentInput = playerInput;
            _playerInControl = playerNb;
            ToggleOnMenu();
        }
    }

    public void ToggleOffMenu()
    {
        StartCoroutine(ToggleOffCoroutine());
    }
    
    public void SkipTuto()
    {
        Time.timeScale = 1;
        if (!_tutoUIManager.TutoEnded)
        {
            _tutoUIManager.TutoEnded = true;
            GameManager.Instance.CleanGame();
            SceneManager.LoadScene(RoundManager.FindNextMap());
        }
        else
        {
            ToggleOffMenu();
        }
    }

    private IEnumerator ToggleOffCoroutine()
    {
        //if(_coroutine != null) StopCoroutine(_coroutine);
        //vPlayer.Play();
        maskAnimator.SetTrigger("Exit");
        yield return new WaitForSecondsRealtime(0.2f);
        maskAnimator.gameObject.SetActive(false);
        //yield return new WaitForSecondsRealtime(1.2f);

        foreach (Player player in Player.ActivePlayers.Values)
        {
            if (player.PlayerNb == _playerInControl) continue;
            player.EnableInputActions();
        }
        Time.timeScale = 1;
        SoundManager.Instance.AudioSourceMusic.UnPause();
        _currentInput.SwitchCurrentActionMap("Player");

        //if (vPlayer.time <= 3.6f) vPlayer.time = 3.6667f;
    }

    private void ToggleOnMenu()
    {
        // vPlayer.time = 0f;
        // vPlayer.Play();
        // _targetImage.texture = vPlayer.texture;
        //_coroutine = StartCoroutine(StopAnim());
        maskAnimator.gameObject.SetActive(true);

        foreach (Player player in Player.ActivePlayers.Values)
        {
            if (player.PlayerNb == _playerInControl) continue;
            player.DisableInputActions();
        }
        SoundManager.Instance.AudioSourceMusic.Pause();
        _currentInput.SwitchCurrentActionMap("UI");
        _currentInput.ActivateInput();
        
        foreach (var uiInput in _uiInputs)
        {
            if (uiInput == null) return;
            uiInput.actionsAsset = _currentInput.actions;
        }
        Time.timeScale = 0;

        EventSystem.current.SetSelectedGameObject(continueButton);
    }

    // private IEnumerator StopAnim()
    // {
    //     // yield return new WaitForSecondsRealtime(1.4f);
    //     // vPlayer.Pause();
    // }

    public void GoBackToMenu()
    {
        Time.timeScale = 1;
        SoundManager.Instance.AudioSourceMusic.UnPause();
        GameManager.Instance.CleanGame();        
        RoundManager.CleanGame();
        SceneManager.LoadScene("Menu");
    }

    public void OnQuit()
    {
        Application.Quit(0);
    }
}
