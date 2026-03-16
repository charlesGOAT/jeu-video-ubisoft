using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject trueBackground;
    [SerializeField] private GameObject falseBackground;
    
    private Toggle _toggle;

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
    }

    public void ValueChanged()
    {
        if (_toggle.isOn)
        {
            trueBackground.SetActive(true);
            // falseBackground.SetActive(false);
        }
        else
        {
            trueBackground.SetActive(false);
            // falseBackground.SetActive(true);
        }
    }
}
