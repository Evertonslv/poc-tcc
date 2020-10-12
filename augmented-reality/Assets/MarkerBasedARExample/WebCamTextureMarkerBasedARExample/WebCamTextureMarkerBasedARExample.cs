using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVMarkerBasedAR;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ArucoModule;
using OpenCVMarkerLessAR;
using OpenCVForUnity.ImgcodecsModule;
using System.IO;
using UnityEngine.SceneManagement;

namespace MarkerBasedARExample
{
    /// <summary>
    /// WebcamTexture Marker Based AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureMarkerBasedARExample : MonoBehaviour
    {
        /// <summary>
        /// The AR camera.
        /// </summary>
        Camera ARCamera;

        /// <summary>
        /// Gameobject armazenas os objetos 3D
        /// </summary>
        GameObject markerList;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffsMarkerLess;

        /// <summary>
        /// The marker detector.
        /// </summary>
        MarkerDetector markerDetector;
            
        /// <summary>
        /// The matrix that inverts the Y axis.
        /// </summary>
        Matrix4x4 invertYM;

        /// <summary>
        /// The matrix that inverts the Z axis.
        /// </summary>
        Matrix4x4 invertZM;
        
        /// <summary>
        /// The transformation matrix.
        /// </summary>
        Matrix4x4 transformationM;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// Lista de objetos 3D
        /// </summary>
        MarkerSettings[] markerSettingsList;

        Mat rgbMat;


        Mat idsAruco;
        List<Mat> cornersAruco;
        Dictionary dictionaryAruco;
        PoseData oldPoseData;


        /// <summary>
        /// The rvecs.
        /// </summary>
        Mat rvecs;

        /// <summary>
        /// The tvecs.
        /// </summary>
        Mat tvecs;

        /// <summary>
        /// The length of the markers' side. Normally, unit is meters.
        /// </summary>
        public float markerLength = 0.1f;

        /// <summary>
        /// The position low pass. (Value in meters)
        /// </summary>
        public float positionLowPass = 0.005f;

        /// <summary>
        /// The rotation low pass. (Value in degrees)
        /// </summary>
        public float rotationLowPass = 2f;

        /// <summary>
        /// The pattern.
        /// </summary>
        Pattern pattern;

        /// <summary>
        /// The pattern tracking info.
        /// </summary>
        PatternTrackingInfo patternTrackingInfo;

        /// <summary>
        /// The pattern mat.
        /// </summary>
        Mat patternMat;

        bool existeObjetoDetectar = true;

        MarkerSettings markerSettingsMarkerLessActual;
        MarkerSettings markerSettingsMarkerActual;

        private void Awake()
        {
            markerList = GameObject.Find("/MarkerList");
            
            CreatComponentMarker();
            CreatComponentMarkerLess();            
        }

        // Use this for initialization
        void Start ()
        {
            GameObject cameraAR = GameObject.Find("ARCamera");
            ARCamera = cameraAR.GetComponent<Camera>();
            markerSettingsMarkerLessActual = null;
            markerSettingsMarkerActual = null;

            patternTrackingInfo = new PatternTrackingInfo();
            markerSettingsList = markerList.transform.GetComponentsInChildren<MarkerSettings>();

            if (markerSettingsList.Length == 0) {
                existeObjetoDetectar = false;
            }

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize();

            dictionaryAruco = Aruco.getPredefinedDictionary(PropertiesModel.DictionaryId);
            cornersAruco = new List<Mat>();
            idsAruco = new Mat();
        }

        // Update is called once per frame
        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);

                foreach (MarkerSettings settings in markerSettingsList)
                {
                    settings.setAllARGameObjectsDisable();
                }

                Aruco.detectMarkers(rgbMat, dictionaryAruco, cornersAruco, idsAruco);

                if (idsAruco.total() > 0)
                {
                    SetMarker();
                }
                else
                {
                    SetMarkerLess();       
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        private void SetMarkerLess()
        {
            if (markerSettingsMarkerLessActual != null && markerSettingsMarkerLessActual.GetPatternDetector().findPattern(rgbMat, patternTrackingInfo))
            {
                ShowGameObjectMarkerLess();
            }
            else
            {
                markerSettingsMarkerLessActual = null;

                foreach (MarkerSettings markerSettings in markerSettingsList)
                {
                    PatternDetector patternDetector = markerSettings.GetPatternDetector();

                    if (patternDetector != null && patternDetector.findPattern(rgbMat, patternTrackingInfo))
                    {
                        markerSettingsMarkerLessActual = markerSettings;
                        ShowGameObjectMarkerLess();
                    }
                }
            }
        }

        private void ShowGameObjectMarkerLess()
        {
            GameObject ARGameObject = markerSettingsMarkerLessActual.getARGameObject();

            if (ARGameObject != null)
            {
                EstimatePoseMarkerLess(ARGameObject);
            }
        }

        private void EstimatePoseMarkerLess(GameObject ARGameObject)
        {
            patternTrackingInfo.computePose(pattern, camMatrix, distCoeffsMarkerLess, rgbMat);
            
            transformationM = patternTrackingInfo.pose3d;
            Matrix4x4 ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertYM;

            ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
            ARGameObject.SetActive(true);
        }

        private void SetMarker()
        {
            for (int i = 0; i < idsAruco.cols(); i++)
            {
                int idMarker = (int)idsAruco.get(0, i)[0];
                Debug.Log(idMarker);

                if (markerSettingsMarkerActual != null && markerSettingsMarkerActual.getId() == idMarker)
                {
                    ShowGameObjectMarker();
                }
                else
                {
                    markerSettingsMarkerActual = null;

                    foreach (MarkerSettings markerSettings in markerSettingsList)
                    {
                        if (idMarker != -1 && idMarker == markerSettings.getId())
                        {
                            markerSettingsMarkerActual = markerSettings;
                            ShowGameObjectMarker();
                        }
                    }
                }
            }
        }

        private void ShowGameObjectMarker()
        {
            GameObject ARGameObjectQrCode = markerSettingsMarkerActual.getARGameObject();

            if (ARGameObjectQrCode != null)
            {
                EstimatePoseMarker(ARGameObjectQrCode);
            }
        }

        private void EstimatePoseMarker(GameObject ARGameObject)
        {
            Aruco.estimatePoseSingleMarkers(cornersAruco, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

            for (int i = 0; i < idsAruco.total(); i++)
            {
                using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                {
                    if (i == 0)
                    {
                        UpdateARObjectTransform(rvec, tvec, ARGameObject);
                    }
                }
            }
        }

        private void UpdateARObjectTransform(Mat rvec, Mat tvec, GameObject ARGameObject)
        {
            // Convert to unity pose data.
            double[] rvecArr = new double[3];
            rvec.get(0, 0, rvecArr);
            double[] tvecArr = new double[3];
            tvec.get(0, 0, tvecArr);
            PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecArr, tvecArr);

            ARUtils.LowpassPoseData(ref oldPoseData, ref poseData, positionLowPass, rotationLowPass);
            oldPoseData = poseData;

            Matrix4x4 matrix = Matrix4x4.TRS(poseData.pos, poseData.rot, new Vector3(0.15f, 0.15f, 0.15f));
            Matrix4x4 ARM = ARCamera.transform.localToWorldMatrix * invertYM * matrix * invertYM;

            ARGameObject.SetActive(true);
            ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
        }

        void CreatComponentMarker() {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(PropertiesModel.NameBDMarkerPlayerPrefab));

            if (informationObjectList == null)
            {
                return;
            }

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject)
            {
                CreateComponent(informationObject, null);
            }
        }

        void CreatComponentMarkerLess()
        {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(PropertiesModel.NameBDMarkerLessPlayerPrefab));

            if(informationObjectList == null)
            {
                return;
            }

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject)
            {
                patternMat = Imgcodecs.imread(informationObject.ImagePathMarkerLess);

                if (patternMat.total() > 0)
                {                    
                    pattern = new Pattern();

                    PatternDetector patternDetector = new PatternDetector(null, null, null, true);
                    patternDetector.buildPatternFromImage(patternMat, pattern);
                    patternDetector.train(pattern);

                    CreateComponent(informationObject, patternDetector);
                }
            }
        }

        void CreateComponent(InformationObject informationObject, PatternDetector patternDetector)
        {
            GameObject ARObjects = new GameObject();
            ARObjects.name = "ARObjects";
            ARObjects.SetActive(false);

            GameObject OBJMarkerSettings = new GameObject();
            OBJMarkerSettings.name = "MarkerSettings";

            MarkerDesign markerDesign = new MarkerDesign();
            markerDesign.id = informationObject.IdMarker;
            
            MarkerSettings markerSettings = OBJMarkerSettings.AddComponent<MarkerSettings>();
            markerSettings.PatternDetector = patternDetector;
            markerSettings.markerDesign = markerDesign;
            markerSettings.ARGameObject = ARObjects;

            GameObject objectAR = Import.GetGameObjectResources(informationObject.Name);
            GameObject objectCreated = Instantiate(objectAR);

            objectCreated.AddComponent<RectTransform>();
            objectCreated.transform.position = Vector3.zero;
            objectCreated.transform.rotation = Quaternion.identity;
            objectCreated.layer = 8;

            objectCreated.transform.SetParent(ARObjects.transform);
            ARObjects.transform.SetParent(OBJMarkerSettings.transform);
            OBJMarkerSettings.transform.SetParent(markerList.transform);            
        }

        private void OnGUI()
        {
            if (!existeObjetoDetectar)
            {
                GUI.Box(new UnityEngine.Rect((Screen.width / 2) - 180, (Screen.height / 2) - 70, 360, 70), "Não existem objetos para ser detectados!");
            }
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized() {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
            
            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
            
            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            
            distCoeffs = new MatOfDouble(0, 0, 0, 0);
            distCoeffsMarkerLess = new MatOfDouble(0, 0, 0, 0);

            //calibration camera
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];
                      
            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2 ((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2 ((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));  
            
            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale) {
                ARCamera.fieldOfView = (float)(fovx [0] * fovXScale);
            } else {
                ARCamera.fieldOfView = (float)(fovy [0] * fovYScale);
            }
            
            MarkerDesign[] markerDesigns = new MarkerDesign[markerSettingsList.Length];
            for (int i = 0; i < markerDesigns.Length; i++) {
                markerDesigns [i] = markerSettingsList[i].markerDesign;
            }

            rvecs = new Mat();
            tvecs = new Mat();

            rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
            transformationM = new Matrix4x4();

            invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
            invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));

            //if WebCamera is frontFaceing,flip Mat.
            webCamTextureToMatHelper.flipHorizontal = webCamTextureToMatHelper.GetWebCamDevice ().isFrontFacing;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode) {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy () {
            webCamTextureToMatHelper.Dispose ();
        }

        public void onBackMainMenu()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
