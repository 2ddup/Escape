using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class FirstLine : MonoBehaviour
{   
    LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform endPoint;
    StopPuzzle stopPuzzle;
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        stopPuzzle = GameObject.FindGameObjectWithTag("StopPuzzle").GetComponent<StopPuzzle>();
    }
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, new Vector3 (startPoint.position.x, startPoint.position.y, stopPuzzle.answerPos[0] + stopPuzzle.cubes[0].transform.localScale.z / 4));
    }

    // Update is called once per frame
    void Update()
    {
        // stopPuzzle.answerPos[0] - stopPuzzle.cubes[0].transform.localScale.x / 2, startPoint.position.y, startPoint.position.z

        // startPoint.position.x, startPoint.position.y, stopPuzzle.answerPos[0] + stopPuzzle.cubes[0].transform.localScale.z / 2
    }
}
