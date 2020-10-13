using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectImport : MonoBehaviour
{
    public void SelectFile()
    {
        CopyFileForResources("C:\\git\\poc-tcc\\augmented-reality\\Assets\\BrokenVector\\LowPolyWinterPack\\Models\\Gift3.fbx");
    }

    private void CopyFileForResources(string pathOrigin)
    {
        if(File.Exists(pathOrigin))
        {
            string pathResources = Application.dataPath + "/Resources";

            File.Copy(pathOrigin, pathResources, true);
            PropertiesModel.NameObjectSelected = "Gift3";

            SceneManager.LoadScene("ObjectSelectMarkerLessScene");
        }
    }

}
