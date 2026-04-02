using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonUpscaler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Button _button;
    private Vector3 _startScale;

    private const float ZOOM_SELECT_MULTIPLIER = 1.3f;

    private void Start()
    {
        _button = GetComponent<Button>();
        _startScale = _button.transform.localScale;
    }

    public void OnSelect(BaseEventData eventData)
    {
        transform.localScale *= ZOOM_SELECT_MULTIPLIER;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.localScale = _startScale;
    }
}