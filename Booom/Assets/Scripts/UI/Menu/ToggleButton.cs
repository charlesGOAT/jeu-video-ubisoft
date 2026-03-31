using System;
using System.Linq;
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

        SetBoolValues();
    }

    private void ValueChanged(bool isOn)
    {
        if (_toggle != null)
            trueBackground.SetActive(_toggle.isOn);
    }

    private void SetBoolValues()
    {
        switch (gameObject.name)
        {
            case "ToggleItems":
                _toggle.isOn = LobbyManager.ItemsActivated;
                break;
            case "ToggleTuto":
                _toggle.isOn = LobbyManager.TutorialActivated;
                break;
            case "ToggleCVD":
                _toggle.isOn = LobbyManager.CVDActivated;
                break;
        }
    }
}
