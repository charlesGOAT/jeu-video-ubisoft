using UnityEngine;

public class AnimationOffseter : MonoBehaviour
{
    private Animator _anim;
    void Awake()
    {
        _anim = GetComponent<Animator>();
        _anim.SetFloat("Offset", Random.Range(0f, 1f));
    }
}
