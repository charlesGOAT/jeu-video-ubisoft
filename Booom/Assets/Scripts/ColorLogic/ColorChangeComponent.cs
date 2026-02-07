using ColorLogic.ColorLogic;
using UnityEngine;  
  
namespace ColorLogic  
{  
    public class ColorChangeComponent : MonoBehaviour  
    {  
        [SerializeField] private Renderer objRenderer;  
  
        void Awake()  
        {            
            if (objRenderer == null)  
            {                
                Debug.LogError("Renderer is not set on object" + gameObject.name + " disabling component...", this);  
                enabled = false;  
            }        
        }
        
        public void ChangeColor(Colors color)  
        {  
            objRenderer.material.color = GetColor(color);  
        }  
  
        private Color GetColor(Colors color)  
        {            
            switch (color)  
            {                
                case Colors.Blue:  
                {  
                    return Color.blue;  
                }                
                case Colors.Green:  
                {  
                    return Color.green;  
                }                
                case Colors.Red:  
                {  
                    return Color.red;  
                }                
                case Colors.Yellow:  
                {  
                    return Color.yellow;  
                }            
            }            
            
            return Color.gray;  
        }    
    }
    
}