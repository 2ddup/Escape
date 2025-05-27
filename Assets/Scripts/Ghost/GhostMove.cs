using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class GhostMove : MonoBehaviour
{
    Transform[] players;
    public float moveSpeed = 0.2f;
    public float rotationSpeed = 2f;
    Vector3 playerToGhost;
    float[] angle;
    bool isEnd = false;

    void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player").Select(obj => obj.transform).ToArray();
        angle = new float[players.Length];
    }

    void Start()
    {
        StartCoroutine(MoveToPlayer(players[0], players[1]));
    }
    void Update()
    {
        if(players.Length < 2)
        {
            Quaternion targetRotation = Quaternion.LookRotation(players[0].position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if(angle.Max() > 65)
            {
                //transform.LookAt(player);
                transform.position = Vector3.MoveTowards(transform.position, players[0].position, moveSpeed * Time.deltaTime);
            }
        }
    }

    Transform CloserPlayer(Transform player1, Transform player2)
    {
        if(Vector3.Distance(transform.position, player1.position) < Vector3.Distance(transform.position, player2.position))
            return player1;
        else
            return player2;
    }
    
    IEnumerator MoveToPlayer(Transform player1, Transform player2)
    {   
        Transform targetPlayer = CloserPlayer(player1, player2);
        float watchTime = 0f;
        
        while(!isEnd)
        {   
            Quaternion targetRotation = Quaternion.LookRotation(targetPlayer.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if(IsPlayerWatching(targetPlayer))
            {
                watchTime += Time.deltaTime;
                if(watchTime > 5f)
                {
                    targetPlayer = (targetPlayer == player1) ? player2 : player1;
                    watchTime = 0f;
                }
            }
            else
            {
                watchTime = 0f;
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    bool IsPlayerWatching(Transform player)
    {
        playerToGhost = new Vector3(transform.position.x - player.position.x, 0, transform.position.z - player.position.z).normalized;

        float angle = Vector3.Angle(player.forward, playerToGhost);
        return angle < 65f;
    }
}
