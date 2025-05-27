using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazePuzzleExplain : MonoBehaviour
{
    Controller controller;
    Text deleteCount;
    // Start is called before the first frame update
    void Awake()
    {
        controller = FindObjectOfType<Controller>();
        deleteCount = transform.Find("RemainCountText").GetComponent<Text>();
    }
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void UpdateDeleteCount()
    {
        deleteCount.text = "남은 벽 제거 횟수 : " +  (4 - controller.deleteCnt) + "회";
    }
}
