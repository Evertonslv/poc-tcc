using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuPrincipal
{
    public class MenuPrincipal : MonoBehaviour
    {

        public void OnObjectCreateWithQrCodeButtonClick()
        {
            SceneManager.LoadScene("ObjectSelectWithQrCodeScene");
        }

        public void OnObjectCreateWithoutQrCodeButtonClick()
        {
            SceneManager.LoadScene("ObjectSelectWithoutQrCodeScene");
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