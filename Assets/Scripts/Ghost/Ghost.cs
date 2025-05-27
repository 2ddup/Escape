using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TempNamespace.Character;
using JetBrains.Annotations;

public class Ghost : MonoBehaviour
{   
    public enum NextBehaviour {None, Move, InvinvibleMove, Attack}
    public NextBehaviour nextBehaviour;
    public Transform[] players;
    public float moveSpeed;
    public float rotationSpeed;
    //public float stareTime;
    //public float invincibleTime;
    public Vector3 playerToGhostDir;
    public Transform targetPlayer;
    public bool stateChanged = false;
    public Animator animator;
    public Animator deadAnimator;
    public PhotonView photonView;
    public GameObject localPlayer;
    public Light[] lights;
    public Material[] ghostMat;
    void Awake()
    {
        photonView = transform.GetComponent<PhotonView>();
        animator = transform.GetComponent<Animator>();
        lights = GetComponentsInChildren<Light>();
        players = GameObject.FindGameObjectsWithTag("Player").Select(obj => obj.transform).ToArray();
        
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (GameObject.FindGameObjectWithTag("RemotePlayer") == null)
        yield return null;

        while (GameObject.FindGameObjectsWithTag("Player").Length < 2)
            yield return null;

        players = GameObject.FindGameObjectsWithTag("Player").Select(obj => obj.transform).ToArray();
        nextBehaviour = NextBehaviour.None;
        targetPlayer = CloserPlayer(players[0],players[1]);
        deadAnimator = GameObject.FindGameObjectWithTag("GameOver").GetComponent<Animator>();
        
        foreach(Light light in lights)
        {
            light.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform CloserPlayer(Transform player1, Transform player2)
    {
        if(Vector3.Distance(transform.position, player1.position) < Vector3.Distance(transform.position, player2.position))
        {
            if(player1.GetComponent<Character>().isDead)
                return player2;
            else
                return player1;
        }
        else
        {   
            if(player2.GetComponent<Character>().isDead)
                return player1;
            else
                return player2;
        }
    }

    public bool IsPlayerWatching(Transform player)
    {
        playerToGhostDir = new Vector3(transform.position.x - player.position.x, 0, transform.position.z - player.position.z).normalized;

        float angle = Vector3.Angle(player.forward, playerToGhostDir);
        return angle < 65f;
    }

    public Transform ReferOtherPlayer(Transform player)
    {
        if(player == players[0])
            return players[1];
        else 
            return players[0];
    }

    public Transform FindNeck(string childName, Transform targetPlayer)
    {
        Transform[] childrenTransform = targetPlayer.GetComponentsInChildren<Transform>();
        foreach (Transform child in childrenTransform)
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }

    [PunRPC]
    public void SyncTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    [PunRPC]
    public void SyncNextBehaviour(int state)
    {
        nextBehaviour = (Ghost.NextBehaviour)state;
    }

    [PunRPC]
    public void SyncTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    [PunRPC]
    public void SyncCharacterState(bool _isDead)
    {
        targetPlayer.GetComponent<Character>().isDead = _isDead;
    }
}
