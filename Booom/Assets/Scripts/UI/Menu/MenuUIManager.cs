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

    public bool isSelectingLevel;
    
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

    private void OnDestroy()
    {
        _lobbyManager.OnLobbyPlayerCountChanged -= UnlockPlayButton;
    }

    private void UnlockPlayButton(int playerCount)
    {
        playButton.interactable = LobbyManager.JoinedPlayers.Count > 1;
        TogglePlayerUI(playerCount);
    }

    private void TogglePlayerUI(int playerCount)
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

    public void ChangeMap()
    {
        isSelectingLevel = !isSelectingLevel;
        mainMenuCanvas.gameObject.SetActive(false);
        levelsCanvas.gameObject.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        isSelectingLevel = !isSelectingLevel;
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
