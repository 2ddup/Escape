using System.Collections;
using System.Collections.Generic;
using CHG.EventDriven;
using DG.Tweening;
using UnityEngine;

public class Toast : MonoBehaviour
{
    public float duration;
    bool isShow = false;
    
    public void Show()
    {
        transform.DOLocalMoveY(500, 1);
        duration = 3;
        isShow = true;
    }
    void Unshow()
    {
        transform.DOLocalMoveY(1500, 1);
        isShow = false;
    }

    void Update()
    {
        if(isShow)
        {
            duration -= Time.deltaTime;
            if(duration <= 0)
                Unshow();
        }
    }
}
