using System.Collections;
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
        nameBDWithQrCodePlayerPrefab = PropertiesModel.NameBDWithQrCodePlayerPrefab;
        nameBDWithoutQrCodePlayerPrefab = PropertiesModel.NameBDWithoutQrCodePlayerPrefab;

        //PlayerPrefs.DeleteAll();
        informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(nameBDWithQrCodePlayerPrefab));

        if (informationObjectList == null)
        {
            informationObjectList = new InformationObjectList();
        }
    }

    private void Start()
    {
        listObjectSelecionado = GameObject.FindGameObjectsWithTag(PropertiesModel.TagMoveObject);
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

        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamic);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fileName = GetFilename();
        imagePath = Path.Combine(directory, fileName);

        if (isWithQrCode)
        {
            imagePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        SaveImage(imageTelaJogo, imagePath);
        SaveInformationObject();

        ObjectScreenVisible(true);
    }

    public string GetFilename()
    {
        string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
        return string.Concat(fileName, ".png");
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
            informationObject.IdMarker = markerIdObject.getIdMarker(objectSelect.name);
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