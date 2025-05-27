using UnityEngine;
using TMPro;

public class Target : MonoBehaviour
{
    public enum TargetColor { Red, Blue, Purple }
    public TargetColor trgtColor;
    private TargetColor TrgtColor
    {
        set
        {
            if(trgtColor == value)
                return;
            
            trgtColor = value;
        }
    }

    public BulletColor bulletColor = BulletColor.None;
    Bullet bullet;
    private Renderer trgtRend;
    private TextMeshPro orderText;
    bool shouldDestroy = false;
    bool isBulletTouched = false;
    private bool redHit = false;
    private bool blueHit = false;
    public int viewID;
    PhotonView photonView;
    public int orderNumber;

    private int OrderNumber
    {
        set {
            if(orderNumber == value)
            {
                return;
            }

            orderNumber = value;
            SetOrderNumber(orderNumber);
        }
    }

    private void Awake()
    {
        trgtRend = GetComponent<Renderer>();
        orderText = GetComponentInChildren<TextMeshPro>();

        if (orderText != null)
        {
            orderText.fontSize = 5;
            orderText.color = Color.white;
            orderText.alignment = TextAlignmentOptions.Center;
            orderText.gameObject.SetActive(false);
        }
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        viewID = photonView.viewID;
        ApplyAppearance();   
    }

    public static int GetRandomTargetColor()
    {
        return Random.Range(0, 3);
    }

    public void Setup(int color, int order)
    {
        trgtColor = (TargetColor)color;
        
        SetOrderNumber(order); // 👈 이걸로 텍스트까지 반영되게
        ApplyAppearance();
    }

    public void SetOrderNumber(int order)  //숫자를 받아서 그 숫자를 타겟 프리펩에 있는 텍스트에 넣고 SetActive하는 함수
    {


        orderNumber = order;

        if (orderText != null)
        {
            orderText.text = orderNumber.ToString();
            if (trgtRend != null && trgtRend.material.color == Color.gray)
            {
                orderText.gameObject.SetActive(true); // OrderViewer일 경우
            }
        }
    }

    public void ApplyAppearance()
    {
        if (PhotonNetwork.isMasterClient)
        {
            UpdateTargetColor();

            if (orderText != null)
                orderText.gameObject.SetActive(false);
        }
        else
        {
            trgtRend.material.color = Color.gray;

            if (orderText != null)
            {
                orderText.text = orderNumber.ToString();
                orderText.gameObject.SetActive(true);
            }
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
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.isWriting)
        {
            stream.SendNext(orderNumber);
            stream.SendNext(trgtColor);
        }
        else
        {
            OrderNumber = (int)stream.ReceiveNext();
            TrgtColor = (TargetColor)stream.ReceiveNext();
        }
    }
    private void OnCollisionEnter(Collision coll)
    {
        if(TargetSequenceManager.Instance.currentIndex == orderNumber - 1)
        {
            bullet = coll.gameObject.GetComponent<Bullet>();
            if (bullet == null) return;

            bulletColor = bullet.bulletColor;
            photonView.RPC("SyncBulletColor",PhotonTargets.Others, (int)bullet.bulletColor);

            isBulletTouched = true;
            photonView.RPC("SyncIsBulletTouched",PhotonTargets.Others, true);             
        }

        Destroy(coll.gameObject);
    }

    private void DestroyCheck(BulletColor _bulletColor)
    {
        shouldDestroy = false;
        switch (trgtColor)
        {
            case TargetColor.Red:
            case TargetColor.Blue:
                if (TargetSequenceManager.Instance != null &&
                    TargetSequenceManager.Instance.IsCorrectTarget(this, _bulletColor))
                {
                    shouldDestroy = true;
                }
                break;

            case TargetColor.Purple:
                if (_bulletColor == BulletColor.Red && TargetSequenceManager.Instance.currentIndex == orderNumber - 1) redHit = true;
                //photonView.RPC("SyncRedHit",PhotonTargets.Others, true);

                if (_bulletColor == BulletColor.Blue && TargetSequenceManager.Instance.currentIndex == orderNumber - 1) blueHit = true;
                //photonView.RPC("SyncBlueHit",PhotonTargets.Others, true);

                //TargetSequenceManager.Instance?.MarkHalfHit(this, _bulletColor);

                if (IsFullyHit() &&
                    TargetSequenceManager.Instance != null &&
                    TargetSequenceManager.Instance.IsCorrectTarget(this, _bulletColor))
                {
                    shouldDestroy = true;
                }
                break;
        }
        
        if (shouldDestroy)
        {
            //photonView.RPC("DestroyTargetRPC", PhotonTargets.Others, viewID);
            ScoreAndDestroy();
        }
    }

    void Update()
    {
        if(isBulletTouched)
            DestroyCheck(bulletColor);
    }

    private void ScoreAndDestroy()
    {
        Vector3 pos = transform.position;
        int score = (trgtColor == TargetColor.Purple) ? 3 : 1;
        ScoreManager.Instance.AddScore(score, pos);

        redHit = false;
        blueHit = false;

        TargetSpawner spawner = FindObjectOfType<TargetSpawner>();
        if (spawner != null)
            spawner.RemoveTargetFromList(gameObject);

        SoundManager.Instance.PlaySFX("TargetDestroy", false);
        //photonView.RPC("DestroyTargetRPC", PhotonTargets.Others, viewID);
        Destroy(gameObject);
    }

    public bool IsFullyHit()
    {
        return redHit && blueHit;
    }
    
    [PunRPC]
    public void DestroyTargetRPC(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            Destroy(pv.gameObject);
        }
    }

    [PunRPC]
    public void SyncShouldDestroy(bool Destroy)
    {
        shouldDestroy = Destroy;
    }

    [PunRPC]
    public void SyncBlueHit(bool _blueHit)
    {
        blueHit = _blueHit;
    }

    [PunRPC]
    public void SyncRedHit(bool _redHit)
    {
        redHit = _redHit;
    }

    [PunRPC]
    public void SyncBulletColor(int _bulletColor)
    {   
        bulletColor = (BulletColor)_bulletColor;
    }

    [PunRPC]
    public void SyncIsBulletTouched(bool isTouched)
    {
        isBulletTouched = isTouched;
    }
}
