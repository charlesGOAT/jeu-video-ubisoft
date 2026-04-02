using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void ChangeFilterCalledEventHandler(int index);

public class CVDButton : MonoBehaviour
{
    [SerializeField] 
    private GameObject toggleGroup;
    
    [SerializeField]
    private Toggle normalFilter;

    private bool _isInitialized = true;

    private string[] _filterNames =
    {
        "PROTANOPIA",
        "PROTANOMALY",
        "DEUTERANOPIA",
        "DEUTERANOMALY",
        "TRITANOPIA",
        "TRITANOMALY",
        "ACHROMATOPSIA",
        "ACHROMATOMALY"
    };
    
    public event ChangeFilterCalledEventHandler OnChangeFilterCalled;

    public void ChangeFilter(int index)
    {
        LobbyManager.CVDIndex = index;
        OnChangeFilterCalled?.Invoke(index + 1);
    }

    public void DisplayColorBlindToggles()
    {
        if (_isInitialized) return;
        var texts = toggleGroup.GetComponentsInChildren<TMP_Text>();
        var toggles = toggleGroup.GetComponentsInChildren<CVDToggle>();

        for(int i = 0; i < _filterNames.Length && i < texts.Length; i++)
        {
            toggles[i].Index = i;
            texts[i].text = _filterNames[i];
        }

        _isInitialized = true;
    }

    public void ResetFilter(bool value)
    {
        if (value)
        {
            var toggles = toggleGroup.GetComponentsInChildren<Toggle>();
            foreach (var toggle in toggles)
            {
                toggle.isOn = true;
            }
            if (LobbyManager.CVDIndex == 0)
                normalFilter.isOn = true;
            else
                toggles[LobbyManager.CVDIndex].isOn = true;
        }
        else
        {
            ChangeFilter(0);
        }
    }
}
