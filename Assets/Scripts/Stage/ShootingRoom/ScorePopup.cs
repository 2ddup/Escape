using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    public TextMeshPro text;

    private float moveSpeed = 0.5f;
    private float duration = 5.0f;
    private float timer = 0f;

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }

        // 항상 카메라를 바라보게
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    public void Setup(int value)
    {
        if(text != null)
        {
            text.text = $"+{value}";
        }
    }
}
