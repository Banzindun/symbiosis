using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    public delegate void DeathDelegate();
    public DeathDelegate OnDeathDelegate;

    [SerializeField]
    private float currentValue;

    [SerializeField] private Animator animator;

    public float CurrentValue
    {
        get { return currentValue; }
        set
        {

            if (currentValue < value)
            {
                OnHeal();
            }
            else if (currentValue > value)
            {
                OnHit();
            }

            currentValue = value;

            if (currentValue <= 0)
            {
                OnDeath();
            }
        }
    }

    private void OnDeath()
    {
        if (OnDeathDelegate != null)
        {
            OnDeathDelegate();
        }
    }

    private void OnHit()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    private void OnHeal()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        currentValue = 1f;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
