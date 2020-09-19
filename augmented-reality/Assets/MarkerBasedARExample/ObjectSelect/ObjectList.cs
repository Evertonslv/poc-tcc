using Assets.MarkerBasedARExample.ObjectSelect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectList : MonoBehaviour
{
    GameObject objectList;

    public GameObject prefab;
    public int numberToCreate;

    void Start()
    {
        //objectList = GameObject.Find("/ListObject");
        GameObject[] listGameObjectResources = Import.GetListGameObjectResources();
        GameObject newObj;

        foreach (GameObject gameObjectResource in listGameObjectResources)
        {
            Instantiate(gameObjectResource, transform);
        }

        //for (int i = 0; i < numberToCreate; i++)
        //{
        //    newObj = Instantiate(prefab, transform);
        //    newObj.GetComponent<Image>().color = Random.ColorHSV();
        //}

    }

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit raycastHit;

        //    if (Physics.Raycast(ray, out raycastHit, 100))
        //    {
        //        if (raycastHit.transform.CompareTag(PropertiesModel.TagMoveObject)) {
        //            PropertiesModel.NameObjectSelected = raycastHit.transform.gameObject.name;
        //            SceneManager.LoadScene("ObjectSelectWithQrCodeScene");
        //        }
        //    }
        //}
    }

    void AddObjectScene(GameObject gameObjectResource)
    {
        GameObject objectCreated = CreateObject.Create(gameObjectResource);
        objectCreated.transform.SetParent(objectList.transform);
    }
}
