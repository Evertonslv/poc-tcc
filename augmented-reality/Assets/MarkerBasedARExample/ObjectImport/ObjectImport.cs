using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectImport : MonoBehaviour
{    
    void Start()
    {
        string path = @"Assets\Desert.unitypackage";
        Import.Package(path);        
    }

    void Update()
    {
        
    }
}
