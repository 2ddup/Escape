using UnityEngine;

public enum BulletColor { Red, Blue, None }

public class Bullet : MonoBehaviour
{
    public BulletColor bulletColor;
    public float speed = 20f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        MoveBullet();
        Destroy(gameObject, 4.0f);
    }

    private void MoveBullet()
    {
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        Destroy(gameObject);
    }
}
