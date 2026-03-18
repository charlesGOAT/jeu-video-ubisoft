using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private PlayerSlot[] playerSlots;
    [SerializeField] private Button playButton;
    [SerializeField] private Canvas mainMenuCanvas;
    
    [SerializeField] private Canvas levelsCanvas;
    [SerializeField] private Image levelPreviewImage;
    
    [SerializeField] private Canvas settingsCanvas;

    public bool isNotMainMenu;
    
    private void Awake()
    {
        playButton.interactable = false;
    }
    
    private void Start()
    {
        LobbyManager.Instance.OnLobbyPlayerCountChanged += UnlockPlayButton;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyPlayerCountChanged -= UnlockPlayButton;
    }

    public void UnlockPlayButton(int playerCount)
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

    public void Settings()
    {
        isNotMainMenu = true;
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
        settingsCanvas.gameObject.SetActive(isNotMainMenu);
    }

    public void ChangeMap()
    {
        isNotMainMenu = true;
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
        levelsCanvas.gameObject.SetActive(isNotMainMenu);
    }

    public void ReturnToMainMenu()
    {
        isNotMainMenu = false;
        levelsCanvas.gameObject.SetActive(isNotMainMenu);
        settingsCanvas.gameObject.SetActive(isNotMainMenu);
        mainMenuCanvas.gameObject.SetActive(!isNotMainMenu);
    }

    public void PlayGame()
    {
        LobbyManager.Instance.GameStarted(levelPreviewImage.sprite.name);
        LobbyManager.Instance.OnLobbyPlayerCountChanged -= UnlockPlayButton;
    }

    public void LevelSelected(Sprite sprite)
    {
        levelPreviewImage.sprite = sprite;
    }

    public void ToggleItems()
    {
        //todo activate or deactivate items
    }
}
