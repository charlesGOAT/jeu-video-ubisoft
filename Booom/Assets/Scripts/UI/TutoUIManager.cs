using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class TutoUIManager : MonoBehaviour
{
    [SerializeField] private GameObject tutoPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text[] tutoText;
    [SerializeField] private Sprite dpadSprite;
    [SerializeField] private Image[]  tutoImages;
    
    [SerializeField] private LocalizeStringEvent[] tutoLocalized;
    [SerializeField] private LocalizedString placeBombInstruction;
    [SerializeField] private LocalizedString moveInstruction;
    [SerializeField] private LocalizedString readyText;
    [SerializeField] private LocalizedString countdownTextLocalized;
    [SerializeField] private LocalizedString fightText;
    
    public bool TutoEnded;

    private void Start()
    {
        foreach (var player in LobbyManager.JoinedPlayers.Keys)
        {
            int index = Player.ActivePlayers.Keys.ToList().IndexOf(player);
            tutoText[index].transform.parent.gameObject.SetActive(true);
            
            tutoLocalized[index].StringReference = placeBombInstruction;
            tutoLocalized[index].RefreshString();
        }
    }

    public void UpdatePlayerText(PlayerEnum player)
    {
        int index = Player.ActivePlayers.Keys.ToList().IndexOf(player);

        tutoImages[index].sprite = dpadSprite;
        
        tutoLocalized[index].StringReference = moveInstruction;
        tutoLocalized[index].RefreshString();
    }

    public void PlayerEndTuto(PlayerEnum player)
    {
        int index = Player.ActivePlayers.Keys.ToList().IndexOf(player);
        
        tutoImages[index].gameObject.SetActive(false);

        RectTransform rt = tutoText[index].GetComponent<RectTransform>();
        Vector3 localPos = rt.localPosition;
        localPos.x = 0;
        rt.localPosition = localPos;

        tutoLocalized[index].StringReference = readyText;
        tutoLocalized[index].RefreshString();
    }

    public void EndTuto()
    {
        if (!TutoEnded)
        {
            TutoEnded = true;
            StartCoroutine(EndTutoCoroutine());
        }
    }
    
    private IEnumerator EndTutoCoroutine()
    {
        tutoPanel.SetActive(true);

        int countdown = 5;

        while (countdown > 0)
        {
            countdownTextLocalized.Arguments = new object[] { countdown };
            countdownText.GetComponent<LocalizeStringEvent>().StringReference = countdownTextLocalized;
            countdownText.GetComponent<LocalizeStringEvent>().RefreshString();

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.GetComponent<LocalizeStringEvent>().StringReference = fightText;
        countdownText.GetComponent<LocalizeStringEvent>().RefreshString();

        yield return new WaitForSeconds(1f);

        GameManager.Instance.CleanGame();
        SceneManager.LoadScene(RoundManager.FindNextMap());
    }
}
