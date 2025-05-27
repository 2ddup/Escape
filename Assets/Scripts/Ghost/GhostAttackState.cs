using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace.Character;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;

public class GhostAttackState : GhostState
{
    Transform ghostHand;
    Transform playerNeck;
    Vector3 offset;
    SkinnedMeshRenderer[] ghostSkinnedMeshRenderer;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        SoundManager.Instance.StopLoopingSFX();
        SoundManager.Instance.PlaySFX("DarkGhostAttack", false);
        if(ghost.targetPlayer.parent.GetComponent<PhotonView>().viewID == GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PhotonView>().viewID)
            ghost.deadAnimator.SetTrigger("Die");

        ghostSkinnedMeshRenderer = ghost.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer SMR in ghostSkinnedMeshRenderer)
        {
            SMR.enabled = true;
        }

        playerNeck = ghost.FindNeck("neck_01", ghost.targetPlayer);
        offset = playerNeck.position - ghost.targetPlayer.position;
        ghostHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        ghost.nextBehaviour = Ghost.NextBehaviour.Move;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {   
        if(stateInfo.normalizedTime <= 0.7f)
            ghost.targetPlayer.position = ghostHand.position - offset;

        if(!ghost.targetPlayer.GetComponent<Character>().isDead && stateInfo.normalizedTime >= 0.6f)
        {
            ghost.targetPlayer.GetComponent<Character>().isDead = true;
            //ghost.photonView.RPC("SyncCharacterState", PhotonTargets.Others, true);
            GlobalEventManager.Instance.Publish("Ragdoll", new RagdollArgs(ghost.targetPlayer.GetComponent<Character>(), true));
        }
        if(stateInfo.normalizedTime >= 0.99)
        {
            ghost.nextBehaviour = Ghost.NextBehaviour.Move;
            animator.SetTrigger("onChange");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.Instance.PlaySFX("DarkGhostMove",true);
    }


}



/*
using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using TempNamespace.Character;
using TempNamespace.EventDriven.Arguments;
using UnityEngine;

public class GhostAttackState : GhostState
{
    Transform ghostHand;
    Transform playerNeck;
    Vector3 offset;
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

        playerNeck = ghost.FindNeck("neck_01", ghost.targetPlayer);
        offset = playerNeck.position - ghost.targetPlayer.position;
        ghostHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if(PhotonNetwork.isMasterClient)
        {
            ghost.nextBehaviour = Ghost.NextBehaviour.Move;
            ghost.photonView.RPC("SyncNextBehaviour", PhotonTargets.Others, (int)Ghost.NextBehaviour.Move);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {   
        if(stateInfo.normalizedTime <= 0.7f)
            ghost.targetPlayer.position = ghostHand.position - offset;

        if(!ghost.targetPlayer.GetComponent<Character>().isDead && stateInfo.normalizedTime >= 0.6f)
        {
            if(PhotonNetwork.isMasterClient)
            {
                ghost.targetPlayer.GetComponent<Character>().isDead = true;
                ghost.photonView.RPC("SyncCharacterState", PhotonTargets.Others, true);
            }
            GlobalEventManager.Instance.Publish("Ragdoll", new RagdollArgs(ghost.targetPlayer.GetComponent<Character>(), true));
        }
        if(stateInfo.normalizedTime >= 0.99)
        {
            if(PhotonNetwork.isMasterClient)
            {
                ghost.nextBehaviour = Ghost.NextBehaviour.Move;
                ghost.photonView.RPC("SyncNextBehaviour", PhotonTargets.Others, (int)Ghost.NextBehaviour.Move);
                animator.SetTrigger("onChange");
                ghost.photonView.RPC("SyncTrigger", PhotonTargets.Others, "onChange");
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
        
    // }
}
*/
