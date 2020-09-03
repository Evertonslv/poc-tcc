using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVMarkerBasedAR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreateMarker : MonoBehaviour
{
    private int dictionaryId = Communs.dictionaryId;
    private int markerSize = 100;
    private Mat markerImage;
    private RawImage imageQrCode;
    private Texture2D texture;

    void Start()
    {
        imageQrCode = FindObjectOfType<RawImage>();
        markerImage = new Mat(markerSize, markerSize, CvType.CV_8UC3);
        texture = new Texture2D(markerImage.cols(), markerImage.rows(), TextureFormat.RGB24, false);
        
        MarkerIdObject markerIdObject = MarkerIdObject.GetInstance();
        Dictionary<string, int> listMarkerIdObject = markerIdObject.getList();
        
        foreach(var markerId in listMarkerIdObject)
        {
            Create(markerId.Value);
        }      
    }

    void Create(int markerId)
    {
        ValidMarkerImage();

        Dictionary dictionary = Aruco.getPredefinedDictionary(dictionaryId);
        Aruco.drawMarker(dictionary, markerId, markerSize, markerImage);

        Utils.matToTexture2D(markerImage, texture, true, 0, true);

        imageQrCode.texture = texture;

        string imagePath = Application.persistentDataPath + "/marker_" + markerId + ".png";
        SaveImage(texture, imagePath);
    }

    private void ValidMarkerImage()
    {
        if (markerImage.cols() != markerSize)
        {
            markerImage.Dispose();
            markerImage = new Mat(markerSize, markerSize, CvType.CV_8UC3);
            texture = new Texture2D(markerImage.cols(), markerImage.rows(), TextureFormat.RGB24, false);
        }
        else
        {
            markerImage.setTo(Scalar.all(255));
        }
    }

    void SaveImage(Texture2D imageSave, string imagePath)
    {
        var bytes = imageSave.EncodeToPNG();
        File.WriteAllBytes(imagePath, bytes);
    }
}