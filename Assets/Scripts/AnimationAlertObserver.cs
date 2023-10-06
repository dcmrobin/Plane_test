using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAlertObserver : MonoBehaviour
{
    [HideInInspector] public bool animationEnded;
    public void AlertObservers(string message)
    {
        if (message.Equals("HoneInAnimationEnds"))
        {
            animationEnded = true;
        }
    }
}
