using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    
    [SerializeField] private PlayerSlot[] playerSlots;
    [SerializeField] private Button playButton;
    [SerializeField] private Canvas mainMenuCanvas;
    
    [SerializeField] private Canvas settingsCanvas;
    
    [SerializeField] private Locale english;
    [SerializeField] private Locale french;

    [SerializeField] private GameObject[] toggles;
    [SerializeField] private GameObject logo;

    public bool isNotMainMenu;
    
    private LobbyManager _lobbyManager;
    
    private void Start()
    {
        _lobbyManager = LobbyManager.Instance;
        LocalizationSettings.SelectedLocale = LobbyManager.TokebaqueIcitte ? french : english;
    }

    private void Update()
    {
        if (LobbyManager.JoinedPlayers.Count > 0 && EventSystem.current.currentSelectedGameObject == null)
        {
            StartCoroutine(ReselectButton());
        }
    }

    public void Settings()
    {
        isNotMainMenu = true;
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
        settingsCanvas.gameObject.SetActive(isNotMainMenu);

        HandleToggleGraffitis(isNotMainMenu);
        logo.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        isNotMainMenu = false;
        settingsCanvas.gameObject.SetActive(isNotMainMenu);
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
        
        HandleToggleGraffitis(isNotMainMenu);
        logo.SetActive(true);
    }

    private void HandleToggleGraffitis(bool value)
    {
        foreach (var toggle in toggles)
        {
            toggle.SetActive(value);
        }
    }

    public void PlayGame()
    {
        if (LobbyManager.JoinedPlayers.Count > 1)
        {
            int mapIndex = LobbyManager.TutorialActivated ? 1 : RoundManager.FindNextMap();
            _lobbyManager.GameStarted(mapIndex);
        }
        else
            StartCoroutine(ReselectButton());
    }
    
    private IEnumerator ReselectButton()
    {
        yield return null;
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    public void ToggleItems()
    {
        LobbyManager.ItemsActivated = !LobbyManager.ItemsActivated;
    }
    
    public void ToggleTuto()
    {
        LobbyManager.TutorialActivated = !LobbyManager.TutorialActivated;
    }
    
    public void ToggleCVD()
    {
        LobbyManager.CVDActivated = !LobbyManager.CVDActivated;
    }

    public void ToggleLanguage()
    {
        LocalizationSettings.SelectedLocale = LobbyManager.TokebaqueIcitte ? english : french;
        LobbyManager.TokebaqueIcitte = !LobbyManager.TokebaqueIcitte;
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
