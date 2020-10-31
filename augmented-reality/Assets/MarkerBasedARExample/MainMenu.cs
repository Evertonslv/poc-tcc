using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public void OnObjectCreateWithMarkerButtonClick()
        {
            PropertiesModel.TypeVisualization = "GenerateMarker";
            SceneManager.LoadScene("ObjectListScene");
        }

        public void OnObjectCreateMarkerLessButtonClick()
        {
            PropertiesModel.TypeVisualization = "GenerateMarker";
            SceneManager.LoadScene("ObjectListScene");
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