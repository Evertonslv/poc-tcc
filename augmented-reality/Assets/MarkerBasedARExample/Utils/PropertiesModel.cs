using UnityEngine;
using OpenCVForUnity.ArucoModule;

public class PropertiesModel : MonoBehaviour
{
    public static string TagMoveObject = "moveObject";
    public static int DictionaryId = Aruco.DICT_ARUCO_ORIGINAL;
    public static string FolderImagemDynamic = "patternImg";
    public static string FolderImagemDynamicOriginal = "patternImg/original";
    public static string FolderImagemDynamicEdge = "patternImg/edge";
    public static string NameBDMarkerPlayerPrefab = "informationObjectWithMarker";
    public static string NameBDMarkerLessPlayerPrefab = "informationObjectMarkerLess";
    public static string NameObjectSelected;
    public static string PathObjectDrawing;
}