using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CreateImage : MonoBehaviour
{
    private GameObject buttonCreateImage;
    private GameObject buttonBackMainMenu;
    private GameObject[] listObjectSelecionado;
    private InformationObjectList informationObjectList;
    private string nameBDPlayerPrefab;
    private string fileName;

    public bool isMarker;

    private void Awake()
    {
        DeletePlayerPrefs();

        nameBDPlayerPrefab = PropertiesModel.NameBDMarkerLessPlayerPrefab;

        if (isMarker)
        {
            nameBDPlayerPrefab = PropertiesModel.NameBDMarkerPlayerPrefab;
        }

        informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(nameBDPlayerPrefab));

        if (informationObjectList == null)
        {
            informationObjectList = new InformationObjectList();
        }
    }

    private void Start()
    {
        fileName = GetFilename();
        listObjectSelecionado = GameObject.FindGameObjectsWithTag(PropertiesModel.TagMoveObject);
        buttonCreateImage = GameObject.Find("/Canvas/GroupButton/btnCreateImage");
        buttonBackMainMenu = GameObject.Find("/Canvas/GroupButton/btnBackMainMenu");
    }
    
    private void DeletePlayerPrefs()
    {
        string directoryEdge = Path.Combine(PropertiesModel.FolderImagemDynamicEdge);

        if (Directory.Exists(directoryEdge)) { 
            Directory.Delete(directoryEdge, true); 
        }

        string directory = Path.Combine(PropertiesModel.FolderImagemDynamicOriginal);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }

        PlayerPrefs.DeleteAll();
    }
    
    IEnumerator CaptureScreenShot()
    {
        yield return new WaitForEndOfFrame();

        ObjectScreenVisible(false);

        // Captura imagem da tela
        Texture2D screenImageTexture = ScreenCapture.CaptureScreenshotAsTexture(2);

        // Salva a imagem original
        string pathObjectOriginal = GetImagePath(true);
        SaveImage(screenImageTexture, pathObjectOriginal);

        // Converte de Texture para Mat do OpenCV
        Mat screenImageMat = new Mat(screenImageTexture.height, screenImageTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(screenImageTexture, screenImageMat);

        // Converte para tons de cinza
        Mat screenImageGrayMat = new Mat(screenImageMat.rows(), screenImageMat.cols(), CvType.CV_8UC4);
        Imgproc.cvtColor(screenImageMat, screenImageGrayMat, Imgproc.COLOR_RGBA2GRAY);

        // Usa o filtro de canny para identificar as bordas
        Mat resultCannyMat = new Mat();
        Imgproc.Canny(screenImageGrayMat, resultCannyMat, 500, 600);

        // Invert as cores
        Mat resultInvertMat = new Mat(resultCannyMat.rows(), resultCannyMat.cols(), CvType.CV_8UC4);
        Core.bitwise_not(resultCannyMat, resultInvertMat);
        
        // Converte Mat para Texture do Unity
        Texture2D resultCannyTexture = new Texture2D(resultInvertMat.cols(), resultInvertMat.rows(), TextureFormat.ARGB32, false);
        Utils.matToTexture2D(resultInvertMat, resultCannyTexture);

        // Salva a imagem em bordas para ser detectado
        PropertiesModel.PathObjectDrawing = GetImagePath(false);
        SaveImage(resultCannyTexture, PropertiesModel.PathObjectDrawing);
        SaveInformationObject();

        Destroy(resultCannyTexture);
        Destroy(screenImageTexture);
    }

    private string GetImagePath(bool original)
    {
        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicEdge);
        
        if (original)
        {
            directory = Path.Combine(Application.dataPath, PropertiesModel.FolderImagemDynamicOriginal);
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, fileName);
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
            informationObject.ImagePathMarkerLess = PropertiesModel.PathObjectDrawing;
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
        buttonBackMainMenu.SetActive(isVisible);
    }
    
    public void OnCreateImageButton()
    {
        if (isMarker)
        {
            SaveInformationObject();
            SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        }
        else 
        {
            StartCoroutine(CaptureScreenShot());
            SceneManager.LoadScene("WebCamDrawingScene");
        }
    }
    
    public void onBackMainMenu()
    {
        PropertiesModel.TypeVisualization = "GenerateMarker";
        SceneManager.LoadScene("ObjectListScene");
    }
}