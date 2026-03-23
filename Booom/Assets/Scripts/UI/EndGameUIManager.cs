using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] 
    private List<Sprite> playerImages = new();

    [SerializeField] 
    private PlayerDisplay[] playerDisplays;

    [SerializeField] 
    private Sprite wonGame;

    public static Dictionary<PlayerEnum, int> PlayerRank = new();
    public static List<PlayerEnum> PlayerWonGame = new();

    private void Awake()
    {
        playerDisplays = GetComponentsInChildren<PlayerDisplay>();
        SetUpUI();
    }

    private void Start()
    {
        //todo : S'assurer que ça joue le endGameMusic
        //SetUpUI();

        StartCoroutine(EndGame());
    }

    private void SetUpUI()
    {
        for(int i = 0; i < playerImages.Count; ++i)
        {
            PlayerEnum playerEnum = (PlayerEnum)(i + 1);
            if (!PlayerRank.TryGetValue(playerEnum, out int rank))
            {
                playerDisplays[i].gameObject.SetActive(false);
                continue;
            }
            
            var playerDisplay = playerDisplays[rank];
            List<Image> images = playerDisplay.images;
            images[0].sprite = playerImages[i];

            for (int j = images.Count - 1; j > PlayerWonGame.Count; j--)
            {
                images[j].gameObject.SetActive(false);
            }

            List<int> gamesWon = PlayerWonGame.Select((x, index) => (Player: x, Index: index + 1)).Where(x => x.Player == playerEnum)
                .Select(x => x.Index).ToList();
            
            foreach(int gameWon in gamesWon)
            {
                images[gameWon].sprite = wonGame;
            }
            
            CenterIcons(playerDisplay);
        }
        
        CenterActivePlayers();
    }

    private void CenterActivePlayers()
    {
        var activePlayers = playerDisplays.Where(p => p.gameObject.activeSelf).ToList();
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

    private void CenterIcons(PlayerDisplay display)
    {
        var activeIcons = display.images.Where(img => img.gameObject.activeSelf).ToList();
        if (activeIcons.Count == 0) return;

        float iconSpacing = 80f;
        float totalIconWidth = (activeIcons.Count - 1) * iconSpacing;
        float iconStartX = -totalIconWidth / 2f;

        for (int i = 1; i < activeIcons.Count; i++)
        {
            RectTransform iconRT = activeIcons[i].GetComponent<RectTransform>();

            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);

            iconRT.anchoredPosition = new Vector2(iconStartX + (i * iconSpacing), iconRT.anchoredPosition.y);
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("Menu");
    }
    
    private void OnDestroy()
    {
        PlayerRank.Clear();
        PlayerWonGame.Clear();
    }
}