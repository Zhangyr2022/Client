using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public const float MinWalkDistance = 0.1f;
    public const float DeadTime = 2f;
    private Animator _animator;
    private void Start()
    {
        this._animator = GetComponent<Animator>();
    }
    /// <summary>
    /// Judge whether to play the walk animation
    /// </summary>
    /// <param name="originalPosition"></param>
    /// <param name="newPosition"></param>
    public void WalkAnimationPlayer(Vector3 originalPosition, Vector3 newPosition)
    {
        if (this._animator == null)
        {
            this._animator = GetComponent<Animator>();
        }

        if (Vector3.Distance(originalPosition, newPosition) > MinWalkDistance * Record.RecordInfo.FrameTime)
            _animator.SetBool("IsWalking", true);
        else
            _animator.SetBool("IsWalking", false);
    }
    /// <summary>
    /// Play the dead animation
    /// </summary>
    public void DeadAnimationPlayer()
    {
        _animator.SetBool("IsDead", true);

        void SetNotDead()
        {
            _animator.SetBool("IsDead", false);
        }
        Invoke(nameof(SetNotDead), DeadTime);
    }
}
