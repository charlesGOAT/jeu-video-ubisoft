using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    
    [SerializeField] private PlayerSlot[] playerSlots;
    [SerializeField] private Button playButton;
    [SerializeField] private Canvas mainMenuCanvas;
    
    [SerializeField] private Canvas settingsCanvas;

    public bool isNotMainMenu;
    
    private LobbyManager _lobbyManager;
    
    private void Start()
    {
        _lobbyManager = LobbyManager.Instance;
    }

    public void TogglePlayerUI(int playerCount)
    {
        var slot = playerSlots[playerCount - 1];

        if (slot.lockedImage.gameObject.activeSelf)
        {
            slot.playerLabel.text = $"Player {playerCount}";
            slot.lockedImage.gameObject.SetActive(false);
            slot.coloredCharacter.gameObject.SetActive(true);
        }
        else
        {
            slot.playerLabel.text = $"Press any button to join";
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
    }

    public void ReturnToMainMenu()
    {
        isNotMainMenu = false;
        settingsCanvas.gameObject.SetActive(isNotMainMenu);
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
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
    
    public void ToggleCVD()
    {
        LobbyManager.CVDActivated = !LobbyManager.CVDActivated;
    }
}
