using UnityEngine;
using UnityEngine.UI;

public class BackgroundFlickerer : MonoBehaviour
{
    [SerializeField]
    private Sprite[] backgrounds;
    
    [SerializeField]
    private float minTime = 0.6f;
    [SerializeField]
    private float maxTime = 0.8f;

    private Image img;
    private int _currentIndex = -1;

    void Start()
    {
        img = GetComponent<Image>();
        Invoke(nameof(Flicker), Random.Range(minTime, maxTime));
    }

    void Flicker()
    {
        int newIndex = _currentIndex;

        while (newIndex == _currentIndex)
        {
            newIndex = Random.Range(0, backgrounds.Length);
        }
        img.sprite = backgrounds[newIndex];
        _currentIndex = newIndex;
        
        Invoke(nameof(Flicker), Random.Range(minTime, maxTime));
    }
}