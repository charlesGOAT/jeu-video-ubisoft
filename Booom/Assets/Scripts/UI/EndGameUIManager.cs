using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct EndGamePlayerData
{
    public int NbGamesWon;
    public int Pos;
    public Color PlayerColor;
}

public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] 
    private List<Sprite> playerImages = new();

    [SerializeField] 
    private List<GameObject> playerDisplays = new();

    [SerializeField] 
    private Sprite wonGame;

    public static Dictionary<PlayerEnum, EndGamePlayerData> PlayerDatas = new();
    public static List<PlayerEnum> PlayerWonGame = new();

    private void Awake()
    {
        SetUpUI();
    }

    private void Start()
    {
        //todo : S'assurer que ça joue le endGameMusic

        StartCoroutine(EndGame());
    }

    private void SetUpUI()
    {
        for(int i = 0; i < playerImages.Count; ++i)
        {
            PlayerEnum playerEnum = (PlayerEnum)(i + 1);
            if (!PlayerDatas.TryGetValue(playerEnum, out EndGamePlayerData playerData))
            {
                playerDisplays[i].gameObject.SetActive(false);
                continue;
            }
            
            GameObject playerDisplay = playerDisplays[playerData.Pos];
            Image[] images = playerDisplay.GetComponentsInChildren<Image>();
            images[0].sprite = playerImages[i];

            for (int j = images.Length - 1; j > PlayerWonGame.Count - 1; j--)
            {
                images[j].gameObject.SetActive(false);
            }

            List<int> gamesWon = PlayerWonGame.Where(x => x == playerEnum).Select((x, index) => (Player: x, Index: index))
                .Select(x => x.Index).ToList();
            
            foreach(int gameWon in gamesWon)
            {
                images[gameWon].sprite = wonGame;
            }

            TextMeshPro text = playerDisplay.GetComponentInChildren<TextMeshPro>();
            text.text = $"Player {i + 1}";
            text.color = playerData.PlayerColor;
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("Menu");
    }
    
    
    private void OnDestroy()
    {
        PlayerDatas.Clear();
    }
}
