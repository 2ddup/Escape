using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopdownFollower : MonoBehaviour
{
    public Transform target;
    public float offset;

    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, offset, target.position.z);
    }
}
