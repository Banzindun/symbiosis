using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [SerializeField] private CustomMonoBehaviour[] enemyBehaviours;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < enemyBehaviours.Length; i++)
        {
            enemyBehaviours[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            Debug.Log("Enabing opponents." + name);

            for (int i = 0; i < enemyBehaviours.Length; i++)
            {
                enemyBehaviours[i].enabled = true;
            }

            Destroy(gameObject);
        }
    }
}
