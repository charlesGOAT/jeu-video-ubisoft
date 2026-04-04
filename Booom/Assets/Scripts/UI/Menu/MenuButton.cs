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
    {// Cache the renderer to avoid calling GetComponent in Update
        if (speaker != null)
        {
            _speakerRenderer = speaker.GetComponent<Renderer>();
        }
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        ApplyMaterial(1);
        _isPressed = true;
        _resetTimer = 0.15f;
    }

    private void ApplyMaterial(int matIndex)
    {
        if (_speakerRenderer == null) return;
        Material[] currentMats = _speakerRenderer.materials;
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
}
