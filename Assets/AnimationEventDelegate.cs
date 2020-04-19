using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventDelegate : MonoBehaviour
{
    [SerializeField]
    private CustomMonoBehaviour owner;

    public void OnAnimationEvent(string name)
    {
        owner.OnAnimationEvent(name);
    }

}
