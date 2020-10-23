using Assets.MarkerBasedARExample.ObjectSelect;
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
        GameObject objectSelected;

        if (PropertiesModel.ImportedExternalObject != null)
        {
            objectSelected = PropertiesModel.ImportedExternalObject;
        }
        else
        {
            if (PropertiesModel.NameObjectSelected == null)
            {
                // objectSelected = SelectObject("Gift1");
                // objectSelected = SelectObject("TreeStump");
                //objectSelected = SelectObject("Sledge");
                objectSelected = SelectObject("Gift3");
                //objectSelected = SelectObject("Cube");
            }
            else
            {
                objectSelected = SelectObject(PropertiesModel.NameObjectSelected);
            }
        }

        if (objectSelected != null)
        {
            if (gameObject.scene.name == "ObjectSelectMarkerLessScene")
            {
                ObjectCreate(objectSelected);
            }
            else
            {
                MarkerIdControl markerIdControl = MarkerIdControl.GetInstance();
                CreateObjectWithIdMarker(objectSelected, markerIdControl.GetMarkerId());
            }
        }
    }

    private GameObject SelectObject(string nameObject)
    {
        return Import.GetGameObjectResources(nameObject);
    }

    private void ObjectCreate(GameObject objectSelected)
    {
        objectCreated = CreateObject.Create(objectSelected);
        objectCreated.transform.SetParent(objectList.transform);
    }

    private void CreateObjectWithIdMarker(GameObject objectSelected, int id)
    {
        ObjectCreate(objectSelected);
        markerIdObject.add(objectCreated.name, id);
    }
}
