using UnityEngine;
using OpenCVForUnity.ArucoModule;
using System.IO;

public class PropertiesModel : MonoBehaviour
{
    public static string TagMoveObject = "moveObject";
    public static int DictionaryId = Aruco.DICT_ARUCO_ORIGINAL;
    public static string FolderImagemDynamic = "patternImg";
    public static string FolderImagemDynamicOriginal = Path.Combine(Application.persistentDataPath, "patternImg/original");
    public static string FolderImagemDynamicEdge = Path.Combine(Application.persistentDataPath, "patternImg/edge");
    public static string NameBDMarkerPlayerPrefab = "informationObjectWithMarker";
    public static string NameBDMarkerLessPlayerPrefab = "informationObjectMarkerLess";
    public static string NameObjectSelected;
    public static string PathObjectDrawing;
    public static string TypeVisualization;
    public static GameObject ImportedExternalObject;
}