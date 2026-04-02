using UnityEngine;
using UnityEngine.UI;


public class CVDToggle : MonoBehaviour
{
    [SerializeField] 
    private CVDButton parent;

    private Toggle _toggle;
    
    public int Index = 0;

    public void Select()
    {
        parent.ChangeFilter(Index);
    }

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
    }
}
