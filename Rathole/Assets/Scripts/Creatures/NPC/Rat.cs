using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Attacker))]
public class Rat : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float jumpTime = 1f;
    [SerializeField] private float minJumpOffset = 0.125f;
    [SerializeField] private float maxJumpOffset = 1f;
    [SerializeField] private float maxAdditionalJumpCooldown = 1f;
    [SerializeField] private float firstJumpCooldown = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D attackColl;
    [SerializeField] private GameObject corpseLeftGO;
    [SerializeField] private GameObject corpseRightGO;

    private Movement movement;
    private Health health;
    private Attacker attacker;

    private GameObject player;
    private bool alive = true;
    private bool dieToTheRight;
    private float nextAttackTime;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        health = GetComponent<Health>();
        attacker = GetComponent<Attacker>();

        //Subscribing to events - PH, should be abstracted to not write this every time for a new enemy
        health.OnKnockbackReceived += movement.StartKnockback;
        health.OnDeath += () => 
        {
            alive = false;
            dieToTheRight = Random.Range(0, 1) == 1;
            animator.SetBool("DieToTheRight", dieToTheRight);
            animator.SetTrigger("Die");
            //animator.gameObject.GetComponent<SpriteRenderer>().flipY = true; // PH
        };

        //Not this one though
        movement.OnStateChange += HandleMovementStateChange;
    }

    private void Start()
    {
        nextAttackTime = Time.time + firstJumpCooldown;
        movement.SetMoveSpeed(walkSpeed);
        player = GameObject.FindWithTag("Player"); //PH (probably not)
    }

    private void Update()
    {
        Vector2 vectorToPlayer = player.transform.position - transform.position;
        switch (movement.GetMovementState())
        {
            case Movement.MovementState.Walking:
                if (!alive) break;

                if (Shortcuts.IsoToReal(vectorToPlayer).sqrMagnitude < 2 * 2 && Time.time >= nextAttackTime && attacker.CanAttack())
                {
                    Vector2 offset = Shortcuts.NormalizeIso(vectorToPlayer) * Random.Range(minJumpOffset, maxJumpOffset);
                    Vector2 jumpTarget = (Vector2)player.transform.position + offset;
                    movement.StartJump(jumpTarget, jumpTime);
                    nextAttackTime = Time.time + attacker.GetAttackCooldown() + Random.Range(0, maxAdditionalJumpCooldown);
                }
                else if (Shortcuts.IsoToReal(vectorToPlayer).sqrMagnitude < 7 * 7)
                {
                    movement.SetTargetMoveDir(Shortcuts.NormalizeIso(vectorToPlayer));
                }
                else
                {
                    movement.SetTargetMoveDir(Vector2.zero);
                }
                break;
            default:
                break;
        }

        //animator.SetFloat("WalkDir", Shortcuts.RealVectorToAnimationDir(Shortcuts.IsoToReal(movement.GetMoveDir()))); //PH
        animator.SetFloat("MoveDir", Shortcuts.RealVectorToAnimationDir(Shortcuts.IsoToReal(movement.GetMoveDir())));
    }

    private void HandleMovementStateChange(Movement.MovementState from, Movement.MovementState to)
    {
        switch (from)
        {
            case Movement.MovementState.Jumping:
                attacker.StopLastingAttack();
                animator.SetBool("IsJumping", false);
                break;
            case Movement.MovementState.KnockedBack:
                //animator.SetBool("KnockedBack", false);
                animator.enabled = true;
                break;
        }

        switch (to)
        {
            case Movement.MovementState.Jumping:
                attacker.StartLastingAttack(attackColl, attackColl.transform, movement.GetMoveDir());
                animator.SetBool("IsJumping", true);
                break;
            case Movement.MovementState.Walking:
                if (!alive)
                {
                    Instantiate(dieToTheRight ? corpseRightGO : corpseLeftGO, transform.position, Quaternion.identity, transform.parent);
                    Destroy(gameObject);
                }
                break;
            case Movement.MovementState.KnockedBack:
                //animator.SetBool("KnockedBack", true);
                animator.enabled = false;
                break;
        }
    }
}