using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDestroyer : MonoBehaviour
{
    GameObject[] players;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (GameObject.FindGameObjectsWithTag("Player").Length < 2)
            yield return null;
        
        players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach(GameObject player in players)
        {
            Destroy(player.GetComponentInChildren<Light>().gameObject);
        }
    }


}
