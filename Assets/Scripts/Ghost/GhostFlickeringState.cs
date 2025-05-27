using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostFlickerState : GhostState
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        foreach(Light light in ghost.lights)
        {
            light.gameObject.SetActive(true);
        }
        foreach(Material mat in ghost.ghostMat)
        {
            mat.color = Color.red;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       foreach(Light light in ghost.lights)
        {
            light.gameObject.SetActive(false);
        }
        
        foreach(Material mat in ghost.ghostMat)
        {
            mat.color = Color.white;
        }
    }
}
