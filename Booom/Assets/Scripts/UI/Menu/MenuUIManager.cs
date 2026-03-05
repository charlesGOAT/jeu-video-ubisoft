using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    
    [SerializeField] private PlayerSlot[] playerSlots;
    [SerializeField] private Button playButton;
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Canvas levelsCanvas;
    [SerializeField] private Image levelPreviewImage;
    
    private LobbyManager _lobbyManager;

    private void Awake()
    {
        playButton.interactable = false;
    }
    
    private void Start()
    {
        _lobbyManager = LobbyManager.Instance;
        _lobbyManager.OnLobbyPlayerCountChanged += UnlockPlayButton;
    }

    private void UnlockPlayButton(int playerCount)
    {
        playButton.interactable = playerCount > 1;
        ShowPlayerJoined(playerCount);
    }

    private void ShowPlayerJoined(int playerCount)
    {
        var slot = playerSlots[playerCount - 1];

        slot.playerLabel.text = $"Player {playerCount}";
        slot.lockedImage.gameObject.SetActive(false);
        slot.coloredCharacter.gameObject.SetActive(true);
    }

    public void ChangeMap()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        levelsCanvas.gameObject.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        levelsCanvas.gameObject.SetActive(false);
        mainMenuCanvas.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        _lobbyManager.GameStarted(levelPreviewImage.sprite.name);
        _lobbyManager.OnLobbyPlayerCountChanged -= UnlockPlayButton;
    }

    public void LevelSelected(Sprite sprite)
    {
        levelPreviewImage.sprite = sprite;
    }
}
