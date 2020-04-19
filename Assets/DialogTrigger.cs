using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{

    [SerializeField] Dialog dialogRoot;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger was entered!!" + other.name);

        if (other.name == "Player")
        {
            DialogManager.Instance.StartDialog(dialogRoot);
            Destroy(gameObject);
        }
    }
}
