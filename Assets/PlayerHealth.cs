using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public delegate void DeathDelegate();
    public DeathDelegate OnDeathDelegate;

    [SerializeField]
    private float currentValue;

    public float CurrentValue
    {
        get { return currentValue; }
        set
        {
            if (value > 0)
            {
                OnHeal();
            }
            else if (value < 0)
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
