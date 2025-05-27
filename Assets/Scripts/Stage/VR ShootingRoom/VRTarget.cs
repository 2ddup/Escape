using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRTarget : MonoBehaviour
{
    public enum TargetColor { Red, Blue, Purple }
    public TargetColor trgtColor;

    private Renderer trgtRend;
    public TextMeshPro orderText;

    private bool redHit = false;
    private bool blueHit = false;

    private int orderNumber;

    private void Awake()
    {
        trgtRend = GetComponent<Renderer>();
        orderText = GetComponentInChildren<TextMeshPro>();

        if (orderText != null)
        {
            orderText.fontSize = 5;
            orderText.color = Color.white;
            orderText.alignment = TextAlignmentOptions.Center;
        }
    }
    public static TargetColor GetRandomTargetColor()
    {
        return (TargetColor)Random.Range(0, 3);
    }

    public void Setup(TargetColor color, int order)
    {
        trgtColor = color;
        SetOrderNumber(order); // 👈 이걸로 텍스트까지 반영되게
        ApplyAppearance();
    }

    public void SetOrderNumber(int order)
    {
        orderNumber = order;

        if (orderText != null)
        {
            orderText.text = orderNumber.ToString();
        }
    }
    public void ApplyAppearance()
    {
        UpdateTargetColor();

        if (orderText != null)
        {
            orderText.text = orderNumber.ToString();
            orderText.gameObject.SetActive(true);
        }

    }

    public void UpdateTargetColor()
    {
        switch (trgtColor)
        {
            case TargetColor.Red:
                trgtRend.material.color = Color.red;
                break;
            case TargetColor.Blue:
                trgtRend.material.color = Color.blue;
                break;
            case TargetColor.Purple:
                trgtRend.material.color = new Color(0.5f, 0f, 0.5f);
                break;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        Bullet bullet = coll.gameObject.GetComponent<Bullet>();
        if (bullet == null) return;

        bool shouldDestroy = false;

        switch (trgtColor)
        {
            case TargetColor.Red:
            case TargetColor.Blue:
                if (VRTargetSequenceManager.Instance != null &&
                    VRTargetSequenceManager.Instance.IsCorrectTarget(this, bullet.bulletColor))
                {
                    shouldDestroy = true;
                }
                break;

            case TargetColor.Purple:
                if (bullet.bulletColor == BulletColor.Red) redHit = true;
                if (bullet.bulletColor == BulletColor.Blue) blueHit = true;

                //VRTargetSequenceManager.Instance?.MarkHalfHit(this, bullet.bulletColor);

                if (IsFullyHit() &&
                    VRTargetSequenceManager.Instance != null &&
                    VRTargetSequenceManager.Instance.IsCorrectTarget(this, bullet.bulletColor))
                {
                    shouldDestroy = true;
                }
                break;
        }

        Destroy(coll.gameObject);

        if (shouldDestroy)
            ScoreAndDestroy();
    }

    private void ScoreAndDestroy()
    {
        Vector3 pos = transform.position;
        int score = (trgtColor == TargetColor.Purple) ? 3 : 1;
        VRScoreManager.Instance.AddScore(score, pos);

        redHit = false;
        blueHit = false;

        VRTargetSpawner spawner = FindObjectOfType<VRTargetSpawner>();
        if (spawner != null)
            spawner.RemoveTargetFromList(gameObject);

        SoundManager.Instance.PlaySFX("TargetDestroy", false);
        Destroy(gameObject);
    }

    public bool IsFullyHit()
    {
        return redHit && blueHit;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
