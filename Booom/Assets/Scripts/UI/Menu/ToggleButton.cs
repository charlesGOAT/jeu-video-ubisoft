using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject trueBackground;
    
    private Toggle _toggle;

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(ValueChanged);
    }

    private void ValueChanged(bool isOn)
    {
        if (_toggle != null)
            trueBackground.SetActive(_toggle.isOn);
    }
}
