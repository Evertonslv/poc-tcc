using Assets.MarkerBasedARExample.ObjectSelect;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectListButton : MonoBehaviour
{
    [SerializeField]
    private RawImage myImage;
    private string myNameImage;

    public void SetImage(Texture2D texture2d)
    {
        myImage.texture = texture2d;
        myNameImage = texture2d.name;
    }

    public void OnClick()
    {
        Debug.Log(myNameImage);

        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicEdge);
        PropertiesModel.PathObjectDrawing = Path.Combine(directory, myNameImage, "png");

        if (PropertiesModel.TypeVisualization == "DrawAgain")
        {
            SceneManager.LoadScene("WebCamDrawingScene");
        }
    }
}
