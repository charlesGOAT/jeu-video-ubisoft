using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimationScript : MonoBehaviour
{
    [SerializeField]
    private AnimationClip _animation;

    private Animator _animator;

    private void Awake()
    {
        if (_animation == null)
        {
            Debug.LogError("Animation component is not assigned in the inspector.");
            return;
        }

        _animator = GetComponent<Animator>();
        _animator.Play(_animation.name);
    }
}




