using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject trueBackground;
    
    private Toggle _toggle;

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
    }

    public void ValueChanged()
    {
        trueBackground.SetActive(_toggle.isOn);
    }
}
