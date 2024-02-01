using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Movement))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private InventoryItem weaponItem;
    [SerializeField] private GameObject attackTrailRotator;
    [SerializeField] private GameObject attackTrail;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator animator;

    private Attacker attacker;
    private Inventory inventory;
    private Collider2D attackTrailColl;
    private Animator attackTrailAnimator;
    private Movement movement;

    private bool hasWeapon = false;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        attacker = GetComponent<Attacker>();
        inventory = GetComponent<Inventory>();
        inventory.OnStoreItems += (InventoryItem item, int count) =>
        {
            if (item == weaponItem)
            {
                hasWeapon = true;
            }
        };

        if (!attackTrail.TryGetComponent<Collider2D>(out attackTrailColl))
        {
            Debug.LogError("attackTrail does not have a Collider2D component");
        }
        
        if (!attackTrail.TryGetComponent<Animator>(out attackTrailAnimator))
        {
            Debug.LogError("attackTrail does not have an Animator component");
        }
    }

    private void Update()
    {
        if (!hasWeapon) return;

        if(InputManager.GetButtonDown(InputManager.InputButton.Attack) || InputManager.GetButton(InputManager.InputButton.Attack))
        {
            if (attacker.CanAttack())
            {
                // Casting camera world pos to Vector2 is necessary so that its Z component doesn't affect the calculation
                Vector2 dir = ((Vector2)playerCamera.ScreenToWorldPoint(Input.mousePosition) - (Vector2)attackTrailRotator.transform.position).normalized;
                Attack(dir);
            }
        }
    }

    private void Attack(Vector2 dir)
    {
        float attackAngle = Vector2.SignedAngle(Vector2.right, dir);
        attackTrailRotator.transform.eulerAngles = Vector3.forward * attackAngle;
        Physics2D.SyncTransforms(); // Force update collider's position according to attackTrailRotator's rotation and scale

        movement.StopWalking();
        attacker.FixedDurationAttackWithEndAction(attackTrailColl, attackTrailRotator.transform, Shortcuts.RealToIso(dir), movement.StartWalking);

        animator.SetTrigger("Attacked");
        float attackAnimDir = Shortcuts.RealVectorToAnimationDir(dir);
        animator.SetFloat("AttackDir", attackAnimDir);
        animator.SetFloat("WalkDir", attackAnimDir);
        attackTrailAnimator.SetTrigger("Attacked");
    }

    //public void Reset()
    //{
    //    hasWeapon = false;
    //}

    public void ResetHasWeapon()
    {
        hasWeapon = inventory.GetItemCount(weaponItem) > 0;
    }
}
