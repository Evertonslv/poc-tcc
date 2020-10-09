using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateImage : MonoBehaviour
{
    private GameObject buttonCreateImage;
    private GameObject[] listObjectSelecionado;
    private InformationObjectList informationObjectList;
    private string nameBDPlayerPrefab;

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

    private void DeletePlayerPrefs()
    {
        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamic);

        if (Directory.Exists(directory)) { 
            Directory.Delete(directory, true); 
        }
        
        PlayerPrefs.DeleteAll();
    }

    private void Start()
    {
        listObjectSelecionado = GameObject.FindGameObjectsWithTag(PropertiesModel.TagMoveObject);
        buttonCreateImage = GameObject.Find("/Canvas/btnCreateImage");
    }

    public void OnCreateImageButton()
    {
        StartCoroutine(CaptureScreenShot());
        //SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        SceneManager.LoadScene("WebCamDrawingScene");
    }

    IEnumerator CaptureScreenShot()
    {
        yield return new WaitForEndOfFrame();

        ObjectScreenVisible(false);

        // Captura imagem da tela
        Texture2D screenImageTexture = ScreenCapture.CaptureScreenshotAsTexture(4);
        
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

        //CriarTexturaTransparente(resultCannyTexture);

        // Salva a imagem para ser detectado
        PropertiesModel.PathObjectDrawing = GetImagePath();
        SaveImage(resultCannyTexture, PropertiesModel.PathObjectDrawing);
        SaveInformationObject();

        Destroy(resultCannyTexture);
        Destroy(screenImageTexture);
    }

    //public Texture2D CriarTexturaTransparente(Texture2D texture)
    //{
    //    Color corTransparente2 = new Color(1.0f, 1.0f, 1.0f, 0f);
    //    Color corTransparente = new Color(1.0f, 1.0f, 1.0f, 0f);

    //    //for (int x = 0; x < texture.height; x++)
    //    //{
    //    //    for (int y = 0; y < texture.width; y++)
    //    //    {
    //    //        texture.SetPixel(x, y, Color.white);
    //    //    }
    //    //}

    //    for (int y = 0; y < texture.height; y++)
    //    {
    //        for (int x = 0; x < texture.width; x++)
    //        {
    //            if (!Color.black.Equals(texture.GetPixel(x, y)))
    //            {
    //                texture.SetPixel(x, y, corTransparente2);
    //            }
    //        }
    //    }

    //    texture.Apply();
    //    SaveImage(texture, GetImagePath());
    //    return texture;
    //}

    private string GetImagePath()
    {
        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamic);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fileName = GetFilename();
        string imagePath = Path.Combine(directory, fileName);

        if (isMarker)
        {
            imagePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        return imagePath;
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
    }
}