using System;
using UnityEngine;

public class ObjectSelect : MonoBehaviour
{    
    GameObject objectList;
    MarkerIdObject markerIdObject;
    GameObject objectCreated;

    void Awake()
    {
        objectList = GameObject.Find("/ListObject");
        markerIdObject = MarkerIdObject.GetInstance();

        GameObject objectSelected = SelectObject("Sledge.fbx");
        //GameObject objectSelected1 = SelectObject("Gift1.fbx");

        if (gameObject.scene.name == "ObjectSelectWithoutQrCodeScene")
        {
            CreateObject(objectSelected);
        }
        else
        {
            MarkerIdControl markerIdControl = MarkerIdControl.GetInstance();
            CreateObjectWithIdMarker(objectSelected, markerIdControl.GetMarkerId());
        }

    }

    private GameObject SelectObject(string nomeObject)
    {
        string objPath2 = string.Concat(Communs.PathFBX, nomeObject);
        return Import.FBX(objPath2);
    }

    private void CreateObject(GameObject objectSelected)
    {
        Quaternion rotation = Quaternion.identity;
        Vector3 position = Vector3.zero;

        objectCreated = Instantiate(objectSelected, position, rotation);
        objectCreated.name = objectSelected.name;

        objectCreated.AddComponent(typeof(MeshCollider));
        objectCreated.AddComponent(typeof(Rigidbody));
        objectCreated.GetComponent<Rigidbody>().isKinematic = true;
        objectCreated.tag = Communs.TagMoveObject;
        objectCreated.transform.SetParent(objectList.transform);
    }

    private void CreateObjectWithIdMarker(GameObject objectSelected, int id)
    {
        CreateObject(objectSelected);
        markerIdObject.add(objectCreated.name, id);
    }
}
