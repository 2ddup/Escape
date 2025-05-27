using System.Collections;
using System.Collections.Generic;
using TempNamespace.Character;
using UnityEngine;

public class GhostMoveState : GhostState
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
        SoundManager.Instance.StopLoopingSFX();
        SoundManager.Instance.PlaySFX("DarkGhostMove",true);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(PhotonNetwork.isMasterClient)
        {
            if(GameObject.FindGameObjectsWithTag("Player").Length >= 2)
            {
                if(ghost.targetPlayer.GetComponent<Character>().isDead)
                {
                    animator.SetTrigger("onChange");
                    ghost.photonView.RPC("SyncTrigger", PhotonTargets.Others, "onChange");
                    ghost.nextBehaviour = Ghost.NextBehaviour.Move;
                    ghost.photonView.RPC("SyncNextBehaviour", PhotonTargets.Others, (int)Ghost.NextBehaviour.Move);
                }
            
                Quaternion targetRotation = Quaternion.LookRotation(ghost.targetPlayer.position - ghost.transform.position);
                ghost.transform.rotation = Quaternion.Slerp(ghost.transform.rotation, targetRotation, ghost.rotationSpeed * Time.deltaTime);
                ghost.transform.position = Vector3.MoveTowards(ghost.transform.position, ghost.targetPlayer.position, ghost.moveSpeed * Time.deltaTime);

                ghost.photonView.RPC("SyncTransform",PhotonTargets.Others, ghost.transform.position, ghost.transform.rotation);

                if(Vector3.Distance(ghost.transform.position, ghost.CloserPlayer(ghost.players[0], ghost.players[1]).position) < 2f)
                {
                    animator.SetTrigger("onChange");
                    ghost.photonView.RPC("SyncTrigger", PhotonTargets.Others, "onChange");
                    ghost.nextBehaviour = Ghost.NextBehaviour.Attack;
                    ghost.photonView.RPC("SyncNextBehaviour", PhotonTargets.Others, (int)Ghost.NextBehaviour.Attack);
                }
                
                if(ghost.IsPlayerWatching(ghost.targetPlayer))
                {
                    animator.SetTrigger("onStop");
                    ghost.photonView.RPC("SyncTrigger", PhotonTargets.Others, "onStop");
                }
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
}
