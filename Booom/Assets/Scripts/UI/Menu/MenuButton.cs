using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    private RaveText _text;
    private void Awake()
    {
        _text = GetComponentInChildren<RaveText>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (EventSystem.current != null)
        {
            _text.isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        }
    }
}
