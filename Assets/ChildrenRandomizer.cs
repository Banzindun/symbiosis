using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenRandomizer : MonoBehaviour
{
    private int lastChoice = 0;

    private void OnEnable()
    {
        lastChoice = lastChoice == 0 ? 1 : 0;
        transform.GetChild(lastChoice).SetAsFirstSibling();
    }

}
