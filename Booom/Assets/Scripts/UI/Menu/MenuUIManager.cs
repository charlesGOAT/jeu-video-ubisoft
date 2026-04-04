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
    
    [SerializeField] private GameObject mainMenuSpeaker;
    [SerializeField] private GameObject settingsSpeaker;
    
    [SerializeField] private Locale english;
    [SerializeField] private Locale french;

    public bool isNotMainMenu;
    
    private LobbyManager _lobbyManager;
    
    private void Start()
    {
        _lobbyManager = LobbyManager.Instance;

        LocalizationSettings.SelectedLocale = english;
    }

    public void TogglePlayerUI(int playerCount)
    {
        var slot = playerSlots[playerCount - 1];

        if (slot.lockedImage.gameObject.activeSelf)
        {
            slot.playerLabel.Arguments = new object[] { playerCount };

            slot.playerLabelLocalized.StringReference = slot.playerLabel;
            slot.playerLabelLocalized.RefreshString();

            slot.lockedImage.gameObject.SetActive(false);
            slot.coloredCharacter.gameObject.SetActive(true);
        }
        else
        {
            slot.playerLabelLocalized.StringReference = slot.joinPrompt;
            slot.playerLabelLocalized.RefreshString();

            slot.lockedImage.gameObject.SetActive(true);
            slot.coloredCharacter.gameObject.SetActive(false);
        }
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
        
        mainMenuSpeaker.gameObject.SetActive(!isNotMainMenu);
        settingsSpeaker.gameObject.SetActive(isNotMainMenu);
    }

    public void ReturnToMainMenu()
    {
        isNotMainMenu = false;
        settingsCanvas.gameObject.SetActive(isNotMainMenu);
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
        
        settingsSpeaker.gameObject.SetActive(isNotMainMenu);
        mainMenuSpeaker.gameObject.SetActive(!isNotMainMenu);
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
        yield return null; // wait one frame
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null; // wait one frame
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
    
    public void ToggleCVD(bool isChecked)
    {
        LobbyManager.CVDActivated = isChecked;
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
