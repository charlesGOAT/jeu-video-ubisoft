using TMPro;
using UnityEngine;

public delegate void ChangeFilterCalledEventHandler(int index);

public class CVDButton : MonoBehaviour
{
    [SerializeField] 
    private GameObject toggleGroup;

    private bool _isInitialized = false;

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
        OnChangeFilterCalled?.Invoke(index);
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
}
