using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelPreview : MonoBehaviour
{
    private Button _levelPreview;

    private const float ZOOM_SELECT_MULTIPLIER = 1.3f;

    private void Awake()
    {
        _levelPreview = GetComponentInChildren<Button>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        transform.localScale *= ZOOM_SELECT_MULTIPLIER;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
