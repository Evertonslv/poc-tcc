using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuPrincipal
{
    public class MenuPrincipal : MonoBehaviour
    {

        public void OnObjectCreateWithQrCodeButtonClick()
        {
            SceneManager.LoadScene("Texture2DToMatExample");
        }

        public void OnObjectCreateWithoutQrCodeButtonClick()
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