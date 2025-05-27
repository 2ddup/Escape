using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WordPuzzle : MonoBehaviour
{
    public List<Transform> puzzlePieces;
    List<Vector3> answerPos = new List<Vector3> {
                                                  new Vector3 (-3.75f, 0f, -0.75f),     //small
                                                  new Vector3 (-3.75f, 0f,  0.75f),     //small
                                                  new Vector3 (  3.0f, 0f, -0.25f),     //med
                                                  new Vector3 (  4.5f, 0f, -0.25f),     //med
                                                  new Vector3 ( -2.0f, 0f,     0f),     //big
                                                  new Vector3 ( -0.5f, 0f,     0f),     //big
                                                  new Vector3 ( 1.37f, 0f,  0.25f),     //tri
                                                  new Vector3 ( 1.13f, 0f, -0.25f)      //tri
                                                };
                                                        
    List<Quaternion> answerRot = new List<Quaternion> {
                                                        Quaternion.Euler( 0f,   0f, 0f),     //small
                                                        Quaternion.Euler( 0f, 180f, 0f),     //small
                                                        Quaternion.Euler( 0f,  90f, 0f),     //big,med
                                                        Quaternion.Euler( 0f, -90f, 0f),     //big,med
                                                        Quaternion.Euler(90f,  90f, 0f),     //tri
                                                        Quaternion.Euler(90f, 270f, 0f)      //tri
                                                      };

    public float errorRange = 0.3f; 
    Camera cam;
    public GameObject selectedObject;
    RaycastHit hit;
    bool isMouseDown;
    public int answerCount = 0;
    Vector3 originPos;
    Quaternion originRot;
    bool isCheck = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        answerCount = 0;

        if(Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition); 
            if(Physics.Raycast(ray, out hit))    
            {
                if(hit.collider.tag == "Word")
                {
                    selectedObject = hit.collider.gameObject;
                    originRot = selectedObject.transform.rotation;
                    originPos = selectedObject.transform.position;
                }
            }
        }
        
        if(selectedObject != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.WorldToScreenPoint(selectedObject.transform.position).z;
            Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
            selectedObject.transform.position = new Vector3(worldPos.x, 0, worldPos.z);
            
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
            if(selectedObject != null)
            {
                if(selectedObject.GetComponent<PuzzlePiece>().col == null)
                {
                    selectedObject = null;
                }
                else 
                {
                    selectedObject.transform.rotation = originRot;
                    selectedObject.transform.position = originPos;
                    selectedObject = null;
                }
            }
        }

        if(isMouseDown)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                float tempRot;
                tempRot = selectedObject.transform.eulerAngles.y;
                tempRot +=90;
                selectedObject.transform.rotation = Quaternion.Euler(selectedObject.transform.eulerAngles.x, tempRot, selectedObject.transform.eulerAngles.z);
            }
        }

        if((Vector3.Distance(puzzlePieces[0].position, answerPos[0]) < errorRange || Vector3.Distance(puzzlePieces[0].position, answerPos[1]) < errorRange) &&
            (puzzlePieces[0].rotation == answerRot[0] || puzzlePieces[0].rotation == answerRot[1])) answerCount++;          //small
        
        if((Vector3.Distance(puzzlePieces[1].position, answerPos[0]) < errorRange || Vector3.Distance(puzzlePieces[1].position, answerPos[1]) < errorRange) &&
            (puzzlePieces[1].rotation == answerRot[0] || puzzlePieces[1].rotation == answerRot[1])) answerCount++;          //small

        if((Vector3.Distance(puzzlePieces[2].position, answerPos[2]) < errorRange || Vector3.Distance(puzzlePieces[2].position, answerPos[3]) < errorRange) &&
            (puzzlePieces[2].rotation == answerRot[2] || puzzlePieces[2].rotation == answerRot[3])) answerCount++;          //med

        if((Vector3.Distance(puzzlePieces[3].position, answerPos[2]) < errorRange || Vector3.Distance(puzzlePieces[3].position, answerPos[3]) < errorRange) &&
            (puzzlePieces[3].rotation == answerRot[2] || puzzlePieces[3].rotation == answerRot[3])) answerCount++;          //med

        if((Vector3.Distance(puzzlePieces[4].position, answerPos[4]) < errorRange || Vector3.Distance(puzzlePieces[4].position, answerPos[5]) < errorRange) &&
            (puzzlePieces[4].rotation == answerRot[2] || puzzlePieces[4].rotation == answerRot[3])) answerCount++;          //big

        if((Vector3.Distance(puzzlePieces[5].position, answerPos[4]) < errorRange || Vector3.Distance(puzzlePieces[5].position, answerPos[5]) < errorRange) &&
            (puzzlePieces[5].rotation == answerRot[2] || puzzlePieces[5].rotation == answerRot[3])) answerCount++;          //big

        if((Vector3.Distance(puzzlePieces[6].position, answerPos[6]) < errorRange || Vector3.Distance(puzzlePieces[6].position, answerPos[7]) < errorRange) &&
            (puzzlePieces[6].rotation == answerRot[4] || puzzlePieces[6].rotation == answerRot[5])) answerCount++;          //tri

        if((Vector3.Distance(puzzlePieces[7].position, answerPos[6]) < errorRange || Vector3.Distance(puzzlePieces[7].position, answerPos[7]) < errorRange) &&
            (puzzlePieces[7].rotation == answerRot[4] || puzzlePieces[7].rotation == answerRot[5])) answerCount++;          //tri
        

        if(answerCount == 8 && !isMouseDown && !isCheck)
        {
            isCheck = true;
            Debug.Log("Solve Puzzle");
        }
    }


}
