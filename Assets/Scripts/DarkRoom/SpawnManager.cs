using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TempNamespace.Character;
using CHG.EventDriven;
using TempNamespace.EventDriven.Arguments;
using System.ComponentModel;
using Unity.VisualScripting;

public class SpawnManager : MonoBehaviour
{
    GameObject player;
    PhotonView photonView;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        while(player == null)
        {
            player = GameObject.FindGameObjectWithTag("LocalPlayer");
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null || !player.GetComponent<PhotonView>().isMine) return ;
     
        var character = player.GetComponentInChildren<Character>();
        if(!character.IsRespawning && character.isDead)
        {   
            StartCoroutine(Respawn(character, 7.0f));
            character.IsRespawning = true;
            //GlobalEventManager.Instance.Publish("Ragdoll", new RagdollArgs(player.GetComponent<Character>(), false));
        }
    }

    IEnumerator Respawn(Character character, float delay)
    {
        var wait = new WaitForEndOfFrame();
        while(delay > 0)
        {
            delay -= Time.deltaTime;
            yield return wait;
        }

        GlobalEventManager.Instance.Publish("Ragdoll", new RagdollArgs(character, false));
        photonView.RPC("RagdollFalse",PhotonTargets.Others, false);
        character.transform.position = transform.position;
        character.isDead = false;
        character.IsRespawning = false;
    }

    
    [PunRPC]
    public void RagdollFalse(bool release)
    {
        GameObject player = GameObject.FindGameObjectWithTag("RemotePlayer");
        Character character = player.GetComponentInChildren<Character>();
        GlobalEventManager.Instance.Publish("Ragdoll",new RagdollArgs(character, release));
    }
    
}
