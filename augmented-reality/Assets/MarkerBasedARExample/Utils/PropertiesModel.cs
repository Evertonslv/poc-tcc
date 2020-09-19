using UnityEngine;
using OpenCVForUnity.ArucoModule;

public class PropertiesModel : MonoBehaviour
{
    public static string TagMoveObject = "moveObject";
    public static int DictionaryId = Aruco.DICT_ARUCO_ORIGINAL;
    public static string FolderImagemDynamic = "patternImg";
    public static string NameBDWithQrCodePlayerPrefab = "informationObjectWithQrCode";
    public static string NameBDWithoutQrCodePlayerPrefab = "informationObjectWithoutQrCode";
    public static string NameObjectSelected;
}