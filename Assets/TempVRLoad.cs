using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempVRLoad : MonoBehaviour
{
    void Start()
    {
        if(PlayerPrefs.GetInt("Clear", 0) == 0)
            this.gameObject.SetActive(false);
    }
    public void LoadVRScene()
    {
        SceneManager.LoadScene("VR ShootingRoom");
    }

}
