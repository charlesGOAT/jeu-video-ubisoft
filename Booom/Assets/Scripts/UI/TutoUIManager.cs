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
    
    private bool _tutoEnded;

    private void Start()
    {
        foreach (var player in LobbyManager.JoinedPlayers.Keys)
        {
            tutoText[(int)player - 1].transform.parent.gameObject.SetActive(true);
        }
    }

    public void UpdatePlayerText(PlayerEnum player)
    {
        tutoText[(int)player - 1].text = $"<sprite name=\"downButton2\"> MOVE";
    }

    public void PlayerEndTuto(PlayerEnum player)
    {
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
