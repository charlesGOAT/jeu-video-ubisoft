using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, ISubmitHandler
{
    [SerializeField]
    private GameObject speaker;
    
    [SerializeField]
    private Material[] materials;
    
    private RaveText _text;
    private Renderer _speakerRenderer;
    private bool _isPressed;
    private float _resetTimer;
    
    private void Awake()
    {
        _text = GetComponentInChildren<RaveText>();
    }

    private void Start()
    {
        if (speaker != null)
        {
            _speakerRenderer = speaker.GetComponent<Renderer>();
        }
        
        SetBoolValues();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        if (gameObject.name == "PlayButton" || gameObject.name == "QuitButton" || gameObject.name == "LanguageButton")
        {
            ApplyMaterial(1);
            _isPressed = true;
            _resetTimer = 0.15f;
        }
        else if (gameObject.name != "SettingButton" && gameObject.name != "ReturnButton")
        {
            StartCoroutine(ValueChanged());
        }
    }

    private void ApplyMaterial(int matIndex)
    {
        if (_speakerRenderer == null) return;
        Material[] currentMats = _speakerRenderer.materials;
        currentMats[3] = materials[matIndex];
        _speakerRenderer.materials = currentMats;
    }
    
    private void ApplyMaterial(bool value)
    {
        if (_speakerRenderer == null) return;
        Material[] currentMats = _speakerRenderer.materials;
        int matIndex = value ? 1 : 0;
        currentMats[3] = materials[matIndex];
        _speakerRenderer.materials = currentMats;
    }

    private void Update()
    {
        if (EventSystem.current != null)
        {
            _text.isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        }
        
        if (_isPressed)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0)
            {
                ApplyMaterial(0);
                _isPressed = false;
            }
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
            case "LanguageButton":
                ApplyMaterial(LobbyManager.TokebaqueIcitte);
                break;
        }
    }
}
