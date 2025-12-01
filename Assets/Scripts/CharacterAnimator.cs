using UnityEngine;

// Updated to avoid naming conflict with UnityEditor.Animations.AnimatorController
[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    private Animator _animator;

    // Animator Parameters Hashes
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int JumpParam = Animator.StringToHash("Jump");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void TriggerJump()
    {
        _animator.SetTrigger(JumpParam);
    }

    public void UpdateState(bool isGrounded, float speed)
    {
        _animator.SetBool(IsGroundedParam, isGrounded);
        _animator.SetFloat(SpeedParam, speed);
    }
}
