using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Building b = BuildingGenerator.Generate();
        Debug.Log(b.ToString());
    }
}
