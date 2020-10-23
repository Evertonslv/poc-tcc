using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public void OnObjectCreateWithMarkerButtonClick()
        {
            SceneManager.LoadScene("ObjectSelectMarkerScene");
        }

        public void OnObjectCreateMarkerLessButtonClick()
        {
            SceneManager.LoadScene("ObjectSelectMarkerLessScene");
        }

        public void OnObjectsCreatedButtonClick()
        {
            SceneManager.LoadScene("WebCamTextureToMatExample");
        }

        public void OnObjectsImportButtonClick()
        {
            SceneManager.LoadScene("ObjectImportScene");
        }

        public void OnObjectAugmentedRealityClick()
        {
            SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        }        
        
        public void OnDrawAgain()
        {
            PropertiesModel.TypeVisualization = "DrawAgain";
            SceneManager.LoadScene("ObjectListScene");
        }
    }
}