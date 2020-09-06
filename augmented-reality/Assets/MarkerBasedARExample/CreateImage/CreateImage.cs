using Boo.Lang;
using OpenCVForUnity.UnityUtils;
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
    private string nameBDWithQrCodePlayerPrefab;
    private string nameBDWithoutQrCodePlayerPrefab;
    private InformationObjectList informationObjectList;
    private string imagePath;

    public bool isWithQrCode;

    private void Awake()
    {
        nameBDWithQrCodePlayerPrefab = Communs.NameBDWithQrCodePlayerPrefab;
        nameBDWithoutQrCodePlayerPrefab = Communs.NameBDWithoutQrCodePlayerPrefab;

        //PlayerPrefs.DeleteAll();
        informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(nameBDWithQrCodePlayerPrefab));

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
        SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
    }

    IEnumerator CaptureScreenShot()
    {
        yield return new WaitForEndOfFrame();

        ObjectScreenVisible(false);
                        
        Texture2D imageTelaJogo = ScreenCapture.CaptureScreenshotAsTexture(2);
        imageTelaJogo.SetPixel(500, 500, Color.gray);
        imageTelaJogo.Apply();

        imagePath = string.Concat(Application.persistentDataPath, Communs.FolderImagemDynamic, "image_screen.png");

        if (isWithQrCode)
        {
            imagePath = string.Concat(Application.persistentDataPath, "/image_screen.png");
        }
        
        SaveImage(imageTelaJogo, imagePath);
        SaveInformationObject();

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

        string nameBDPlayerPrefab = nameBDWithoutQrCodePlayerPrefab;

        if (isWithQrCode)
        {
            nameBDPlayerPrefab = nameBDWithQrCodePlayerPrefab;
        }

        PlayerPrefs.SetString(nameBDPlayerPrefab, informationList);
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
            informationObject.ImagePathWithoutQrCode = imagePath;
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