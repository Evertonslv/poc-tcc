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
        public Camera ARCamera;

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
        /// The transformation matrix for AR.
        /// </summary>
        Matrix4x4 ARM;

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

        private void Awake()
        {
            markerList = GameObject.Find("/MarkerList");
            
            CreatComponentWithQrCode();
            CreatComponentWithoutQrCode();            
        }

        // Use this for initialization
        void Start ()
        {
            patternTrackingInfo = new PatternTrackingInfo();
            markerSettingsList = markerList.transform.GetComponentsInChildren<MarkerSettings>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
            webCamTextureToMatHelper.Initialize();

            dictionaryAruco = Aruco.getPredefinedDictionary(Communs.DictionaryId);
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
                    for (int i = 0; i < idsAruco.cols(); i++)
                    {
                        int idMarker = (int)idsAruco.get(0, i)[0];
                        Debug.Log(idMarker);

                        foreach (MarkerSettings settings in markerSettingsList)
                        {
                            if (idMarker == settings.getId())
                            {
                                GameObject ARGameObjectQrCode = settings.getARGameObject();

                                if (ARGameObjectQrCode != null)
                                {
                                    EstimatePoseCanonicalMarker(rgbMat, ARGameObjectQrCode);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (MarkerSettings settings in markerSettingsList)
                    {
                        PatternDetector patternDetector = settings.GetPatternDetector();

                        if (patternDetector != null && patternDetector.findPattern(rgbMat, patternTrackingInfo))
                        {
                            Debug.Log("Encontrou imagem");

                            patternTrackingInfo.computePose(pattern, camMatrix, distCoeffs);
                            transformationM = patternTrackingInfo.pose3d;

                            GameObject ARGameObject = settings.getARGameObject();

                            if (ARGameObject != null)
                            {
                                EstimatePoseCanonicalMarker(rgbMat, ARGameObject);
                            }
                        }
                    }
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        private void EstimatePoseCanonicalMarker(Mat rgbMat, GameObject ARGameObject)
        {
            Aruco.estimatePoseSingleMarkers(cornersAruco, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

            if (idsAruco.total() > 0)
            {
                for (int i = 0; i < idsAruco.total(); i++)
                {
                    using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                    using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                    {
                        // In this example we are processing with RGB color image, so Axis-color correspondences are X: blue, Y: green, Z: red. (Usually X: red, Y: green, Z: blue)
                        Calib3d.drawFrameAxes(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);

                        // This example can display the ARObject on only first detected marker.
                        if (i == 0)
                        {
                            UpdateARObjectTransform(rvec, tvec, ARGameObject);
                        }
                    }
                }
            }
            else
            {
                ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;
                
                ARGameObject.SetActive(true);
                ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
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
            
            // Convert to transform matrix.
            ARM = ARUtils.ConvertPoseDataToMatrix(ref poseData, true);
            ARM = ARCamera.transform.localToWorldMatrix * ARM;

            ARGameObject.SetActive(true);
            ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
        }

        void CreatComponentWithQrCode() {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(Communs.NameBDWithQrCodePlayerPrefab));

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject) {
                CreateComponent(informationObject, null);
            }
        }

        void CreatComponentWithoutQrCode()
        {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(Communs.NameBDWithoutQrCodePlayerPrefab));

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject)
            {
                patternMat = Imgcodecs.imread(informationObject.ImagePathWithoutQrCode);

                if (patternMat.total() > 0)
                {
                    Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_BGR2RGB);

                    Texture2D patternTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

                    //To reuse mat, set the flipAfter flag to true.
                    Utils.matToTexture2D(patternMat, patternTexture, true, 0, true);
                    
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
            markerDesign.id = informationObject.IdMeker;

            MarkerSettings markerSettings = OBJMarkerSettings.AddComponent<MarkerSettings>();
            markerSettings.PatternDetector = patternDetector;
            markerSettings.markerDesign = markerDesign;
            markerSettings.ARGameObject = ARObjects;

            string pathFBX = string.Concat(Communs.PathFBX, informationObject.Name, Communs.ExtensionFBX);
            GameObject objectAR = Import.FBX(pathFBX);
            GameObject objectCreated = Instantiate(objectAR);

            objectCreated.AddComponent<RectTransform>();
            objectCreated.transform.position = Vector3.zero;
            objectCreated.transform.rotation = Quaternion.identity;
            objectCreated.layer = 8;

            RectTransform rectTransform = objectCreated.GetComponent<RectTransform>();

            float widthScale = (Screen.width / rectTransform.rect.width) + 10;
            float heightScale = (Screen.height / rectTransform.rect.height) + 10;
            objectCreated.transform.localScale = new Vector3(widthScale, heightScale);

            objectCreated.transform.SetParent(ARObjects.transform);
            ARObjects.transform.SetParent(OBJMarkerSettings.transform);
            OBJMarkerSettings.transform.SetParent(markerList.transform);            
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

            markerDetector = new MarkerDetector (camMatrix, distCoeffs, markerDesigns);

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
    }
}
