using TMPro;
using UnityEngine;

public class ScorePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScore;

    public int currentScore = 0;
    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
        playerScore.text = score.ToString();
    }

    public void SetColor(Color c)
    {
        playerScore.color = c;
    }
}
