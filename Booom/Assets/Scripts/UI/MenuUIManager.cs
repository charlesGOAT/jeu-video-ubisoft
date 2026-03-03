using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    
    [SerializeField] private PlayerSlot[] playerSlots;
    [SerializeField] private Button playButton;
    
    private LobbyManager _lobbyManager;

    private void Start()
    {
        playButton.interactable = false;
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        _lobbyManager = LobbyManager.Instance;
        
        _lobbyManager.OnLobbyPlayerCountChanged += UnlockPlayButton;
    }

    private void UnlockPlayButton(int playerCount)
    {
        playButton.interactable = playerCount > 1;
        ShowPlayerJoined(playerCount);
    }

    public void ShowPlayerJoined(int playerCount)
    {
        var slot = playerSlots[playerCount - 1];

        slot.playerLabel.text = $"Player {playerCount}";
        slot.lockedImage.gameObject.SetActive(false);
        slot.coloredCharacter.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        _lobbyManager.GameStarted();
        _lobbyManager.OnLobbyPlayerCountChanged -= UnlockPlayButton;
    }
}
