using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.HasWon())
            {
                GameManager.Instance.OnPlayerWon();
            }
            else
            {
                tooltipText.text = "Power is low. Eat more enemies.";
            }
        }
    }

    private void OnTriggerExit(Collider2D other)
    {
        tooltipText.text = "";
    }
}
