using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HOHOHAHA : MonoBehaviour
{
    public Image image;
    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Alpha(float duration)
    {
        image.DOColor(Color.white, duration);
    }
}
