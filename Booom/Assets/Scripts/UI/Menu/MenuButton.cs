using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, ISubmitHandler
{
    [SerializeField]
    private GameObject quadGameObject;
    
    [SerializeField]
    private Material[] materials;
    
    private RaveText _text;
    private Renderer _quadRenderer;
    
    private void Awake()
    {
        _text = GetComponentInChildren<RaveText>();
    }

    private void Start()
    {
        if (quadGameObject != null)
        {
            _quadRenderer = quadGameObject.GetComponent<Renderer>();
        }
        
        SetBoolValues();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        if (gameObject.name == "TutorialButton" || gameObject.name == "ItemsButton")
        {
            StartCoroutine(ValueChanged());
        }
    }
    
    private void ApplyMaterial(bool value)
    {
        if (_quadRenderer == null) return;
        int matIndex = value ? 0 : 1;
        _quadRenderer.material = materials[matIndex];
    }

    private void Update()
    {
        if (EventSystem.current != null)
        {
            _text.isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        }
    }

    private IEnumerator ValueChanged()
    {
        yield return null;
        SetBoolValues();
    }
    
    private void SetBoolValues()
    {
        switch (gameObject.name)
        {
            case "ItemsButton":
                ApplyMaterial(LobbyManager.ItemsActivated);
                break;
            case "TutorialButton":
                ApplyMaterial(LobbyManager.TutorialActivated);
                break;
        }
    }
}
