using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TutoUIManager : MonoBehaviour
{
    [SerializeField] private GameObject tutoPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text[] tutoText;
    [SerializeField] private TMP_SpriteAsset secondSpriteAsset;
    
    private bool _tutoEnded;

    private void Start()
    {
        foreach (var player in LobbyManager.JoinedPlayers.Keys)
        {
            tutoText[(int)player - 1].transform.parent.gameObject.SetActive(true);
            tutoText[(int)player - 1].spriteAsset.fallbackSpriteAssets.Add(secondSpriteAsset);
        }
    }

    public void UpdatePlayerText(PlayerEnum player)
    {
        tutoText[(int)player - 1].text = $"<sprite name=\"dpad\"> MOVE AND FILL THE REST OF YOUR ZONE";
    }

    public void PlayerEndTuto(PlayerEnum player)
    {
        RectTransform rt = tutoText[(int)player - 1].GetComponent<RectTransform>();
        Vector3 localPos = rt.localPosition;
        localPos.x = 0;
        rt.localPosition = localPos;
        
        tutoText[(int)player - 1].text = "YOU'RE READY !";
    }

    public void EndTuto()
    {
        if (!_tutoEnded)
        {
            _tutoEnded = true;
            StartCoroutine(EndTutoCoroutine());
        }
    }
    
    private IEnumerator EndTutoCoroutine()
    {
        tutoPanel.SetActive(true);
        
        int countdown = 5;

        while (countdown > 0)
        {
            countdownText.text = $"GET READY TO FIGHT IN {countdown}...";
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "FIGHT!";
        yield return new WaitForSeconds(1f);

        GameManager.Instance.NewRound();

        SceneManager.LoadScene(RoundManager.FindNextMap());
    }
}
