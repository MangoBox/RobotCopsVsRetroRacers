using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityNameGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    public string[] beginningNames;
    public string[] middleNames;
    public string[] endNames;
    
    public string GetRandomCityName() {
        string begin = beginningNames[Random.Range(0,beginningNames.Length)];
        string middle = middleNames[Random.Range(0,middleNames.Length)];
        string end = endNames[Random.Range(0,endNames.Length)];

        return begin + middle + end;
    }
}
