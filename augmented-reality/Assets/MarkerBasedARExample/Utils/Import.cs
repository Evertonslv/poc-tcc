using UnityEngine;

public class Import
{    
    public static GameObject GetGameObjectResources(string nameObject) 
    {
        return Resources.Load<GameObject>(nameObject);
    }

    public static GameObject[] GetListGameObjectResources()
    {
        return Resources.LoadAll<GameObject>("");
    }

}