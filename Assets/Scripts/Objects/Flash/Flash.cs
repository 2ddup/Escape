using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Flash : MonoBehaviour
{   
    Light flashLight;
    public string[] excludeLayer = {"RedLight", "BlueLight","NoLight"};
    // Start is called before the first frame update
    void Awake()
    {
        flashLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    [ContextMenu("ExcludeRed")]
    public void ExcludeRedInCullingMask()
    {
        int layertoexclude = LayerMask.NameToLayer(excludeLayer[0]);
        flashLight.cullingMask &= ~(1 << layertoexclude);
    }

    [ContextMenu("ExcludeBlue")]
    public void ExcludeBlueInCullingMask()
    {
        int layertoexclude = LayerMask.NameToLayer(excludeLayer[1]);
        flashLight.cullingMask &= ~(1 << layertoexclude);
    }
    
    [ContextMenu("ExcludeNone")]
    public void ExcludeNoInCullingMask()
    {
        int layertoexclude = LayerMask.NameToLayer(excludeLayer[2]);
        flashLight.cullingMask &= ~(1 << layertoexclude);
    }

    public void IncludeAllInCullingMask()
    {
        flashLight.cullingMask = -1;   
    }
}
