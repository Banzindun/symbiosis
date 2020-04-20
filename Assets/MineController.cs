using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;

public class MineController : CustomMonoBehaviour
{
    [SerializeField] private Vector3[] strike;
    [SerializeField] private Animator animator;
    [SerializeField] private int damage;

    public override void OnTurnStart() { }
    public override void OnPlayerTurn() { }
    public override void OnEnemyAttack() { }
    public override void OnEnemyMove() { }
    public override void OnTurnEnd() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("MINE: Collided with " + other.name);

        if (other.gameObject.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        animator.SetTrigger("Explode");
        MusicManager.Instance.Play("MineExplosion");
    }

    public override void OnAnimationEvent(string name)
    {
        if (name == "ActionDone")
        {
            Destroy(gameObject);
        }
        else if (name == "Exploded")
        {
            for (int i = 0; i < strike.Length; i++)
            {
                Vector3 strikePosition = transform.position + strike[i];
                Health enemyHealth = Utils.GetComponentAtPosition2D<Health>(strikePosition);

                if (enemyHealth != null)
                {
                    enemyHealth.CurrentValue -= damage;
                }
            }
        }
    }
}
