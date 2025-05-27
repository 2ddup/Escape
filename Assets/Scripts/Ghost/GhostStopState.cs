using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostStopState : GhostState
{
    SkinnedMeshRenderer[] ghostSkinnedMeshRenderer;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        
        ghostSkinnedMeshRenderer = ghost.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer SMR in ghostSkinnedMeshRenderer)
        {
            SMR.enabled = true;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Quaternion targetRotation = Quaternion.LookRotation(ghost.targetPlayer.position - ghost.transform.position);
        ghost.transform.rotation = Quaternion.Slerp(ghost.transform.rotation, targetRotation, ghost.rotationSpeed * Time.deltaTime);

        ghost.nextBehaviour = Ghost.NextBehaviour.InvinvibleMove;

        if(!ghost.IsPlayerWatching(ghost.targetPlayer))
        {
            animator.SetTrigger("onMove");
            ghost.nextBehaviour = Ghost.NextBehaviour.None;
        }

        if(Vector3.Distance(ghost.transform.position, ghost.CloserPlayer(ghost.players[0], ghost.players[1]).position) < 2f)
        {
            animator.SetTrigger("onChange");
            ghost.nextBehaviour = Ghost.NextBehaviour.Attack;
        }

        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
}
