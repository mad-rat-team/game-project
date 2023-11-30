using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] GameObject attackTrailRotator;
    [SerializeField] GameObject attackTrail;
    [SerializeField] Camera playerCamera;
    [SerializeField] ContactFilter2D enemiesContactFilter;

    private Collider2D attackTrailColl;
    private Animator attackTrailAnimator;

    private void Awake()
    {
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
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            // Casting camera world pos to Vector2 is necessary so that its Z component doesn't affect the calculation
            Vector2 dir = ((Vector2)playerCamera.ScreenToWorldPoint(Input.mousePosition) - (Vector2)attackTrailRotator.transform.position).normalized;
            Attack(dir);
        }

        //Debug.DrawRay(
        //    attackTrail.transform.position,
        //    ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)(attackTrailRotator.transform.position)).normalized,
        //    Color.white
        //    );
    }

    private void Attack(Vector2 dir)
    {
        //Debug.Log($"Attacked to {dir}");
        float attackAngle = Vector2.SignedAngle(Vector2.right, dir);

        attackTrailRotator.transform.eulerAngles = Vector3.forward * attackAngle;

        //attackTrailRotator.transform.localScale =
        //    new Vector3(
        //        attackTrailRotator.transform.localScale.x,
        //        Mathf.Abs(attackAngle) > 90 ? -1 : 1,
        //        attackTrailRotator.transform.localScale.z
        //        );

        Physics2D.SyncTransforms(); // Force update collider's position according to attackTrailRotator's rotation and scale
        List<Collider2D> enemyColliders = new List<Collider2D>();
        attackTrailColl.OverlapCollider(enemiesContactFilter, enemyColliders);

        foreach (var enemyColl in enemyColliders)
        {
            NPCHealth enemyHealth = enemyColl.GetComponentInParent<NPCHealth>();
            enemyHealth.TakeHit(attackTrailRotator.transform.position, 3f, 0.5f); //PH
        }

        attackTrailAnimator.SetTrigger("Attacked");
    }
}
