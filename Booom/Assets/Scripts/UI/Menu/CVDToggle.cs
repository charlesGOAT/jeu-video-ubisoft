using UnityEngine;


public class CVDToggle : MonoBehaviour
{
    [SerializeField] 
    private CVDButton parent; 
    
    public int Index = 0;

    public void Select()
    {
        parent.ChangeFilter(Index);
    }
}
