using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeListPopup : MonoBehaviour
{
    public FuelManager fuel;
    public Text fuelCount;

    // Start is called before the first frame update
    void Start()
    {
        if(fuel == null)
        {
            fuel = FindAnyObjectByType<FuelManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        int currentFuel = fuel.fuelCount;
        fuelCount.text = $"자동차 연료의 갯수 : {currentFuel}";

    }
}
