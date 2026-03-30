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

    public void UpdatePlayerText(PlayerEnum player)
    {
        tutoText[(int)player - 1].text = "SPREAD YOUR COLOR TO FILL ALL THE TILES";
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
