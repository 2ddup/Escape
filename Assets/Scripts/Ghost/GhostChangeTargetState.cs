using System.Collections;
using System.Collections.Generic;
using TempNamespace.Character;
using UnityEngine;

public class GhostChangeTargetState : GhostState
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        
        ghost.targetPlayer = (ghost.targetPlayer == ghost.players[0]) ? ghost.players[1] : ghost.players[0];

        if(ghost.nextBehaviour == Ghost.NextBehaviour.Attack)
            ghost.targetPlayer = ghost.CloserPlayer(ghost.players[0], ghost.players[1]);           

        if(ghost.targetPlayer.gameObject.GetComponent<Character>().isDead)
            ghost.targetPlayer = ghost.ReferOtherPlayer(ghost.targetPlayer);

        switch (ghost.nextBehaviour)
        {
            case Ghost.NextBehaviour.Move:
                animator.SetTrigger("onMove");
                ghost.nextBehaviour = Ghost.NextBehaviour.None;
                break;

            case Ghost.NextBehaviour.InvinvibleMove:
                animator.SetTrigger("onInvincibleMove");
                ghost.nextBehaviour = Ghost.NextBehaviour.None;
                break;

            case Ghost.NextBehaviour.Attack:
                animator.SetTrigger("onAttack");
                ghost.nextBehaviour = Ghost.NextBehaviour.None;
                break;

            case Ghost.NextBehaviour.None:
                break;    
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    // }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
}
