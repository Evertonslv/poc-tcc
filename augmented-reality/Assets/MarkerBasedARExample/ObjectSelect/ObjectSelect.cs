using System;
using UnityEngine;

public class ObjectSelect : MonoBehaviour
{    
    GameObject objectList;
    MarkerIdObject markerIdObject;

    void Awake()
    {
        objectList = GameObject.Find("/ListObject");
        markerIdObject = MarkerIdObject.GetInstance();

        GameObject objectSelected = SelectObject("Sledge.fbx");
        CreateObject(objectSelected, 33);

        // GameObject objectSelected1 = SelectObject("Gift1.fbx");
        // CreateObject(objectSelected1, 1);        
    }

    private GameObject SelectObject(string nomeObject)
    {
        string objPath2 = String.Concat(Communs.PathFBX, nomeObject);
        return Import.FBX(objPath2);
    }

    private void CreateObject(GameObject objectSelected, int id)
    {
        Quaternion rotation = Quaternion.identity;
        Vector3 position = Vector3.zero;

        GameObject objectCreated = Instantiate(objectSelected, position, rotation) as GameObject;
        objectCreated.name = objectSelected.name;

        objectCreated.AddComponent(typeof(MeshCollider));
        objectCreated.AddComponent(typeof(Rigidbody));
        objectCreated.GetComponent<Rigidbody>().isKinematic = true;
        objectCreated.tag = Communs.TagMoveObject;
        objectCreated.transform.SetParent(objectList.transform);

        markerIdObject.add(objectCreated.name, id);
    }
}
