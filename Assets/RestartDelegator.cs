using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartDelegator : MonoBehaviour
{
    public void Restart()
    {
        GameManager.Instance.RestartGame();
    }
}
