using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Movement))]
//[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private Animator animator;

    private Movement movement;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    private void Start()
    {
        movement.SetMoveSpeed(moveSpeed);
    }

    private void Update()
    {
        movement.SetTargetMoveDir(Shortcuts.RealToIso(GetInputMoveDir()));

        animator.SetFloat("WalkDir", Shortcuts.GetAnimationDir(movement.GetWalkDir()));
        animator.SetBool("IsWalking", movement.GetMovementState() == Movement.MovementState.Walking && !movement.IsMoving());
    }

    /// <summary>
    /// Gets input move dir.
    /// </summary>
    /// <returns>
    /// <b>Non-iso-vector</b> of movement.
    /// </returns>
    private Vector2 GetInputMoveDir()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }
}