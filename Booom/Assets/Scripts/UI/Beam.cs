using UnityEngine;

public class Beam : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject, 1f);
    }
}
