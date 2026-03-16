using TMPro;
using UnityEngine;

public delegate void ChangeFilterCalledEventHandler(int index);

public class CVDButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    private int _index;

    private string[] _filterNames =
    {
        "PRESS TO CHANGE",
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

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        _index = 0;
        _text.text = _filterNames[_index];
        OnChangeFilterCalled?.Invoke(_index);
    }
    
    public void ChangeFilter()
    {
        if (_index < _filterNames.Length - 1)
        {
            _index++;
        }
        else
        {
            _index = 0;
        }
        
        _text.text = _filterNames[_index];
        OnChangeFilterCalled?.Invoke(_index);
        
    }
}
