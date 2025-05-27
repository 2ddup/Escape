using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyButt : MonoBehaviour, IPointerEnterHandler
{
    public Animator ani;

    public string hover = "Hover";

    public void OnPointerEnter(PointerEventData eventData)
    {
        ani.SetTrigger(hover);

        SoundManager.Instance.PlaySFX("LobbyHover", false);
    }
}