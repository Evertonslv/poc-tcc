using Boo.Lang;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateImage : MonoBehaviour
{    
    private GameObject buttonCreateImage;
    private GameObject[] listObjectSelecionado;
    private string namePlayerPrefab = "informationObject";
    private InformationObjectList informationObjectList;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString("informationObject"));

        if (informationObjectList == null)
        {
            informationObjectList = new InformationObjectList();
        }
    }

    private void Start()
    {
        listObjectSelecionado = GameObject.FindGameObjectsWithTag(Communs.TagMoveObject);
        buttonCreateImage = GameObject.Find("/Canvas/btnCreateImage");
    }

    public void OnCreateImageButton()
    {
        StartCoroutine(CaptureScreenShot());
        SaveInformationObject();
        SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
    }

    IEnumerator CaptureScreenShot()
    {
        yield return new WaitForEndOfFrame();

        ObjectScreenVisible(false);
        
        Texture2D imageTelaJogo = ScreenCapture.CaptureScreenshotAsTexture(2);

        string imagePath = String.Concat(Application.persistentDataPath, "/image_screen.png");
        SaveImage(imageTelaJogo, imagePath);

        ObjectScreenVisible(true);
    }

    void SaveImage(Texture2D imageSave, string imagePath)
    {
        var bytes = imageSave.EncodeToPNG();
        File.WriteAllBytes(imagePath, bytes);
    }

    void SaveInformationObject()
    {
        SetInformationObject();
        string informationList = JsonUtility.ToJson(informationObjectList);
        PlayerPrefs.SetString(namePlayerPrefab, informationList);
        PlayerPrefs.Save();
    }

    private void SetInformationObject()
    {
        InformationObject informationObject;
        MarkerIdObject markerIdObject = MarkerIdObject.GetInstance();

        foreach (GameObject objectSelect in listObjectSelecionado)
        {
            informationObject = new InformationObject();
            informationObject.Name = objectSelect.name;
            informationObject.IdMeker = markerIdObject.getIdMarker(objectSelect.name);
            informationObject.Position = objectSelect.transform.position;
            informationObject.Rotation = objectSelect.transform.rotation;
            informationObject.Scale = objectSelect.transform.localScale;

            informationObjectList.ListInformationObject.Add(informationObject);
        }
    }

    void ObjectScreenVisible(bool isVisible)
    {
        buttonCreateImage.SetActive(isVisible);
    }
}