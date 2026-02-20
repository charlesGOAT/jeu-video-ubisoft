using TMPro;
using UnityEngine;

public class ScorePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScore;

    public void UpdateScore(int score)
    {
        playerScore.text = score.ToString();
    }

    public void SetColor(Color c)
    {
        playerScore.color = c;
    }
}
