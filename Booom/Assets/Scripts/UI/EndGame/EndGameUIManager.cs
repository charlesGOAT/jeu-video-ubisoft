using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[Serializable]
public struct BombPlayer
{
    public EndGameBombAnimation bomb1;
    public EndGameBombAnimation bomb2;
}
public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] 
    private List<BombPlayer> bombs = new();

    [SerializeField] 
    private Sprite wonGame;

    [SerializeField] private GameObject grafittis1;
    [SerializeField] private GameObject grafittis2;
    [SerializeField] private GameObject crown;

    [SerializeField] 
    private GameObject nextRoundButton;
    [SerializeField] 
    private GameObject backToMenuButton;

    public static Dictionary<PlayerEnum, int> PlayerRank = new();
    public static List<PlayerEnum> PlayerWonGame = new();
    public static int NextSceneIndex = 0;
    public static bool ShouldEndGame = false;

    private PlayerDisplay[] _displays;

    private static bool _playersDisappeared;

    public static GameObject FirstSelected;

    private void Awake()
    {
        if (ShouldEndGame)
        {
            nextRoundButton.SetActive(false);
            FirstSelected = backToMenuButton;
            grafittis1.SetActive(false);
            grafittis2.SetActive(true);
            crown.SetActive(true);
        }
        else
        {
            FirstSelected = nextRoundButton;
        }
        EventSystem.current.SetSelectedGameObject(FirstSelected);
    }

    private void Start()
    {
        StartCoroutine(AppearPlayers());
        SetUpUI();
    }

    private void SetUpPlayer(Player player)
    {
        float yRotation = 0;
        switch (PlayerRank[player.PlayerNb])
        {
            case 0:
                yRotation = -125f;
                break;
            case 1:
                yRotation = -105f;
                break;
            case 2:
                yRotation = -75f;
                break;
            case 3:
                yRotation = -55f;
                break;
            default:
                Debug.LogWarning("Maximum of " + GameConstants.NB_PLAYERS + " players reached. Extra device ignored.");
                return;
        }

        player.gameObject.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void SetUpUIEndGame()
    {
        nextRoundButton.SetActive(false);
        FirstSelected = backToMenuButton;
        //EventSystem.current.SetSelectedGameObject(backToMenuButton);
    }
    private void SetUpUI()
    {
        //EventSystem.current.SetSelectedGameObject(nextRoundButton);

        foreach(var (playerEnum, rank) in PlayerRank)
        {
            Color playerColor = Player.PlayerColorDict[playerEnum];
            var bombsPlayer = bombs[rank];
            bombsPlayer.bomb1.gameObject.SetActive(true);
            bombsPlayer.bomb2.gameObject.SetActive(true);

            int gamesWon = PlayerWonGame.Count(x => x == playerEnum);

            if (gamesWon == 1)
            {
                bombsPlayer.bomb1.SetBombColor(playerColor);
                bombsPlayer.bomb1.InitializeAnimation(0.5f);
            }
            else if(gamesWon == 2)
            {
                bombsPlayer.bomb1.SetBombColor(playerColor);
                bombsPlayer.bomb1.InitializeAnimation(0f);
                bombsPlayer.bomb2.SetBombColor(playerColor);
                bombsPlayer.bomb2.InitializeAnimation(0.5f);
            }
        }
    }

    private void CenterActivePlayers()
    {
        var activePlayers = _displays.Where(p => p.gameObject.activeSelf).ToList();
        if (activePlayers.Count == 0) return;

        float playerSpacing = 500f;
        float totalPlayerWidth = (activePlayers.Count - 1) * playerSpacing;
        float playerStartX = -totalPlayerWidth / 2f;

        for (int i = 0; i < activePlayers.Count; i++)
        {
            RectTransform playerRT = activePlayers[i].GetComponentInChildren<RectTransform>();
        
            playerRT.anchorMin = new Vector2(0.5f, 0.5f);
            playerRT.anchorMax = new Vector2(0.5f, 0.5f);
            playerRT.pivot = new Vector2(0.5f, 0.5f);
        
            playerRT.anchoredPosition = new Vector2(playerStartX + (i * playerSpacing), playerRT.anchoredPosition.y);
        }
    }

    private void CenterIcons(in PlayerDisplay display)
    {
        var activeIcons = display.images.Where(img => img.gameObject.activeSelf).ToList();
        if (activeIcons.Count == 0) return;

        float iconSpacing = 80f;
        float totalIconWidth = (activeIcons.Count - 1) * iconSpacing;
        float iconStartX = -totalIconWidth / 2f;

        for (int i = 1; i < activeIcons.Count; i++)
        {
            var parent = activeIcons[i].transform.parent.GetComponent<RectTransform>();
            parent.sizeDelta = new(200, 100);
            RectTransform iconRT = activeIcons[i].GetComponent<RectTransform>();
            iconRT.sizeDelta = new(100, 100);

            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);

            iconRT.anchoredPosition = new Vector2(iconStartX + (i * iconSpacing), iconRT.anchoredPosition.y);
        }
    }
    
    private void DisappearPlayers()
    {
        if (_playersDisappeared) return;
        foreach (var player in LobbyManager.JoinedPlayers)
        {
            player.Value.transform.position = new Vector3(999,0,999);
        }

        _playersDisappeared = true;
    }

    private IEnumerator AppearPlayers()
    {
        yield return null;
        foreach (var player in LobbyManager.JoinedPlayers)
        {
            var p = player.Value.GetComponent<Player>();
            Player.ActivePlayers[player.Key] = p;
            
            Vector2Int spawnPointGrid = GameManager.Instance.GridManager.playerSpawnPoints[PlayerRank[p.PlayerNb]];
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(spawnPointGrid);

            tile.IsSpawn = false;
            if(GameManager.Instance.IsSpreadingMode)
                tile.ChangeTileColor(p.PlayerNb);
        
            MovePlayerOnSpawnPoint(p, spawnPointGrid);
            SetUpPlayer(p);
        }
        _playersDisappeared = false;
    }

    private void MovePlayerOnSpawnPoint(Player player, Vector2Int gridPos)
    {
        Vector3 worldPos = GridManagerStrategy.GridToWorldPosition(gridPos);
        var trans = player.gameObject.transform;
        trans.position = new Vector3(worldPos.x, 0, worldPos.z);
       player.CurrentTile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridPos);
    }
    
    private void OnDestroy()
    {
        PlayerRank.Clear();
        PlayerWonGame.Clear();
        ShouldEndGame = false;
    }

    public void NextRound() // todo hide ce bouton si c'était la dernière round
    {
        if (ShouldEndGame) SceneManager.LoadScene("Menu");
        else SceneManager.LoadScene(NextSceneIndex);
    }
    
    public void GoBackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit(0);
    }
    
}
