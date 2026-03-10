using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelPreviewButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private MenuUIManager menuUIManager;
    
    private LevelPreview _parent;
    private Image _image;

    private void Awake()
    {
        _parent = GetComponentInParent<LevelPreview>();
        _image = GetComponent<Image>();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        _parent.OnSelect();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _parent.OnDeselect();
    }

    public void LevelSelected()
    {
        menuUIManager.LevelSelected(_image.sprite);
        menuUIManager.ReturnToMainMenu();
    }
}