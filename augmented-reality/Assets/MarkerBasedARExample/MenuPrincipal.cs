using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuPrincipal
{
    public class MenuPrincipal : MonoBehaviour
    {

        public void OnObjectSelectButtonClick()
        {
            SceneManager.LoadScene("Texture2DToMatExample");
        }

        public void OnObjectsCreatedButtonClick()
        {
            SceneManager.LoadScene("WebCamTextureToMatExample");
        }

        public void OnObjectsImportButtonClick()
        {
            SceneManager.LoadScene("WebCamTextureToMatHelperExample");
        }

        public void OnObjectViewButtonClick()
        {
            SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        }
    }
}