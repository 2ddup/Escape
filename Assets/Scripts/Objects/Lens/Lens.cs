using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TempNamespace;
using UnityEngine;

public class Lens : MonoBehaviour
{
    public enum LensColor {Red, Blue, White};
    public LensColor lensColor;
    Light myLight;
    public Material[] lens;
    PhotonView photonView;
    int itemId;
    Item item;
    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        item = GetComponent<Item>();
        itemId = item.itemId;
    }
    private void OnEnable()
    {
        Item.OnItemPickedUp += PickUpLens;
        InventoryUI.OnItemUse += UseLens;
    }

    private void OnDisable()
    {
        Item.OnItemPickedUp -= PickUpLens;
        InventoryUI.OnItemUse -= UseLens;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PickUpLens(int _itemId, int __)
    {
        if (_itemId != itemId) return;

        if(item.pickedItemObject != this.gameObject) return;

        switch(_itemId)
        {
            case 1:                         //White
                PickUpWhiteLens();
                photonView.RPC("SyncPickUpWhiteLens", PhotonTargets.Others);
                break;

            case 2:                         //Red
                PickUpRedLens();
                photonView.RPC("SyncPickUpRedLens", PhotonTargets.Others);
                break;
            
            case 3:                         //Blue
                PickUpBlueLens();
                photonView.RPC("SyncPickUpBlueLens", PhotonTargets.Others);
                break;
        }
        SoundManager.Instance.PlaySFX("DarkLensGet", false);
    }

    void PickUpRedLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    void PickUpBlueLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    void PickUpWhiteLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    [PunRPC]
    public void SyncPickUpRedLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    [PunRPC]
    public void SyncPickUpBlueLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    [PunRPC]
    public void SyncPickUpWhiteLens()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }


    void UseLens(int _itemId)
    {
        if (_itemId != itemId) return;

        switch(_itemId)
        {
            case 1:                         //White
                UseWhiteLens();
                photonView.RPC("SyncUseWhiteLens", PhotonTargets.Others);
                break;

            case 2:                         //Red
                UseRedLens();
                photonView.RPC("SyncUseRedLens", PhotonTargets.Others);
                break;
            
            case 3:                         //Blue
                UseBlueLens();
                photonView.RPC("SyncUseBlueLens", PhotonTargets.Others);
                break;
        }
        SoundManager.Instance.PlaySFX("DarkLensUse", false);
    }

    void UseRedLens()
    {
        myLight = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Light>();
        myLight.color = Color.red;

        Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
        mats[5] = lens[0];
        myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;

        myLight.cullingMask = -1;
        myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
        myLight.gameObject.GetComponent<Flash>().ExcludeBlueInCullingMask();
    }

    void UseBlueLens()
    {
        myLight = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Light>();
        myLight.color = Color.blue;

        Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
        mats[5] = lens[1];
        myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;

        myLight.cullingMask = -1;
        myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
        myLight.gameObject.GetComponent<Flash>().ExcludeRedInCullingMask();
    }

    void UseWhiteLens()
    {
        myLight = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Light>();
        myLight.color = Color.white;

        Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
        mats[5] = lens[2];
        myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;
        
        myLight.cullingMask = -1;
        myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
        myLight.gameObject.GetComponent<Flash>().ExcludeBlueInCullingMask();
        myLight.gameObject.GetComponent<Flash>().ExcludeRedInCullingMask();
    }

    [PunRPC]
    public void SyncUseBlueLens()
    {
        Light avatarLight = GameObject.FindGameObjectWithTag("RemotePlayer").GetComponentInChildren<Light>();

        avatarLight.color = Color.blue;
        avatarLight.cullingMask = -1;

        Flash flash = avatarLight.GetComponent<Flash>();
        flash.ExcludeNoInCullingMask();
        flash.ExcludeRedInCullingMask();
    }

    [PunRPC]
    public void SyncUseRedLens()
    {
        Light avatarLight = GameObject.FindGameObjectWithTag("RemotePlayer").GetComponentInChildren<Light>();

        avatarLight.color = Color.red;
        avatarLight.cullingMask = -1;

        Flash flash = avatarLight.GetComponent<Flash>();
        flash.ExcludeNoInCullingMask();
        flash.ExcludeBlueInCullingMask();
    }

    [PunRPC]
    public void SyncUseWhiteLens()
    {
        Light avatarLight = GameObject.FindGameObjectWithTag("RemotePlayer").GetComponentInChildren<Light>();

        avatarLight.color = Color.white;
        avatarLight.cullingMask = -1;

        Flash flash = avatarLight.GetComponent<Flash>();
        flash.ExcludeNoInCullingMask();
        flash.ExcludeBlueInCullingMask();
        flash.ExcludeRedInCullingMask();
    }
    // void PickUpRedLens(int itemId)
    // {
    //     myLight.color = Color.red;

    //     Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
    //     mats[5] = lens[0];
    //     myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;

    //     myLight.cullingMask = -1;
    //     myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
    //     myLight.gameObject.GetComponent<Flash>().ExcludeBlueInCullingMask();

    //     gameObject.SetActive(false);
    // }

    // void PickUpBlueLens(int itemId)
    // {     
    //     myLight.color = Color.blue;

    //     Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
    //     mats[5] = lens[1];
    //     myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;

    //     myLight.cullingMask = -1;
    //     myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
    //     myLight.gameObject.GetComponent<Flash>().ExcludeRedInCullingMask();
        
    //     gameObject.SetActive(false);
    // }

    // void PickUpWhiteLens(int itemId)
    // {
    //     myLight.color = Color.white;

    //     Material[] mats = myLight.gameObject.GetComponent<MeshRenderer>().materials;
    //     mats[5] = lens[2];
    //     myLight.gameObject.GetComponent<MeshRenderer>().materials = mats;
        
    //     myLight.cullingMask = -1;
    //     myLight.gameObject.GetComponent<Flash>().ExcludeNoInCullingMask();
    //     myLight.gameObject.GetComponent<Flash>().ExcludeBlueInCullingMask();
    //     myLight.gameObject.GetComponent<Flash>().ExcludeRedInCullingMask();
        
    //     gameObject.SetActive(false);
    // }

    
}
