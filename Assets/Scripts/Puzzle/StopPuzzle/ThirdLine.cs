using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ThirdLine : MonoBehaviour
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
        lineRenderer.SetPosition(0, new Vector3 (startPoint.position.x, startPoint.position.y, stopPuzzle.answerPos[1] - stopPuzzle.cubes[0].transform.localScale.z / 4));
        lineRenderer.SetPosition(1, endPoint.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
