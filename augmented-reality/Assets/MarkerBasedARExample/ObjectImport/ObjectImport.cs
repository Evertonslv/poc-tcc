using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assimp;
using Assets.MarkerBasedARExample.ObjectImport;

public class ObjectImport : MonoBehaviour
{
    //void Start()
    //{
    //    SelectFile();
    //}

    public void SelectFile()
    {
        string pathResource = @"C:\git\poc-tcc\augmented-reality\Assets\BrokenVector\LowPolyWinterPack\Models\Gift3.fbx";
        //StartCoroutine(CopyFileForResources(pathResource));
        CopyFileForResources(pathResource);
    }

    void CopyFileForResources(string pathOrigin)
    {
        if (File.Exists(pathOrigin))
        {
            //string pathResources = Application.streamingAssetsPath;
            string pathResources = @"C:\git\poc-tcc\augmented-reality\Assets\MarkerBasedARExample\Resources\Gift3.fbx";

            File.Copy(pathOrigin, pathResources, true);
            PropertiesModel.NameObjectSelected = pathOrigin;
            //string url = $"file:///{pathOrigin}";

            using (AssimpContext assimp = new AssimpContext())
            {
                assimp.SetConfig(new Assimp.Configs.MeshVertexLimitConfig(60000));
                assimp.SetConfig(new Assimp.Configs.MeshTriangleLimitConfig(60000));
                assimp.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
                assimp.SetConfig(new Assimp.Configs.SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Line | Assimp.PrimitiveType.Point));

                PostProcessSteps postProcessSteps = PostProcessSteps.SplitLargeMeshes |
                                                                    PostProcessSteps.OptimizeGraph |
                                                                    PostProcessSteps.OptimizeMeshes |
                                                                    PostProcessSteps.Triangulate |
                                                                    PostProcessSteps.SortByPrimitiveType |
                                                                    PostProcessPreset.TargetRealTimeMaximumQuality |
                                                                    PostProcessSteps.FlipWindingOrder;

                Assimp.Scene scene = assimp.ImportFile(pathOrigin, postProcessSteps);
                print("rootNode name: " + scene.RootNode.Name);

                MainNodeFBX mainNodeFBX = new MainNodeFBX();
                PropertiesModel.ImportedExternalObject = new GameObject(scene.RootNode.Name);

                ImportNode(scene.RootNode, mainNodeFBX, scene, PropertiesModel.ImportedExternalObject);

                DontDestroyOnLoad(PropertiesModel.ImportedExternalObject);
                SceneManager.LoadScene("ObjectSelectMarkerLessScene");
            }

        }
    }

    void ImportNode(Node refNode, MainNodeFBX newNode, Assimp.Scene model, GameObject go)
    {
        if (refNode.HasChildren)
        {

            //create new array of node children
            newNode.children = new MainNodeFBX[refNode.ChildCount];
            print(" node: " + refNode.Name + " nombre d'enfant: " + refNode.ChildCount);
            print("type du node: " + refNode.GetType().ToString());

            for (int i = 0; i < refNode.ChildCount; i++)
            {

                //create the child node
                newNode.children[i] = new MainNodeFBX();
                //create the game object for the node
                GameObject childGo = new GameObject();
                MeshFilter mf = childGo.AddComponent(typeof(MeshFilter)) as MeshFilter;
                MeshRenderer mr = childGo.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                childGo.transform.parent = go.transform;
                childGo.name = refNode.Children[i].Name;

                //Set transform 
                Assimp.Vector3D position, scaling;
                Assimp.Quaternion rotation;
                refNode.Transform.Decompose(out scaling, out rotation, out position);
                childGo.transform.localPosition = new Vector3(position.X, position.Z, position.Y);
                childGo.transform.localRotation = new UnityEngine.Quaternion(rotation.X, rotation.Z, rotation.Y, rotation.W);
                childGo.transform.localScale = new Vector3(scaling.X, scaling.Z, scaling.Y);

                //recursive methode to get all the children node

                ImportNode(refNode.Children[i], newNode.children[i], model, childGo);

            }

        }

        if (refNode.HasMeshes)
        {
            //create array of meshes of this
            newNode.meshes = new UnityEngine.Mesh[refNode.MeshCount];

            print("final node : " + refNode.Name + "a comme enfant: " + refNode.MeshCount);
            print("type du node: " + refNode.GetType().ToString());

            //create the game object for the mesh
            /* */

            for (int i = 0; i < refNode.MeshCount; i++)
            {
                GameObject childGo = new GameObject();
                MeshFilter mf = childGo.AddComponent(typeof(MeshFilter)) as MeshFilter;
                MeshRenderer mr = childGo.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                childGo.transform.parent = go.transform;
                childGo.name = i.ToString();
                print("nb faces: " + model.Meshes[refNode.MeshIndices[i]].FaceCount);
                //set tranform
                Assimp.Vector3D position, scaling;
                Assimp.Quaternion rotation;
                refNode.Transform.Decompose(out scaling, out rotation, out position);
                childGo.transform.localPosition = new Vector3(position.X, position.Z, -position.Y);
                childGo.transform.localRotation = new UnityEngine.Quaternion(rotation.X, rotation.Z, rotation.Y, rotation.W);
                childGo.transform.localScale = new Vector3(scaling.X, scaling.Z, scaling.Y);

                List<Vector3> verticeTmp = new List<Vector3>();

                //get vertices from model
                Vector3 VectTmp = new Vector3();
                for (int j = 0; j < model.Meshes[refNode.MeshIndices[i]].VertexCount; j++)
                {

                    VectTmp = new Vector3(model.Meshes[refNode.MeshIndices[i]].Vertices[j].X,
                        model.Meshes[refNode.MeshIndices[i]].Vertices[j].Z,
                        model.Meshes[refNode.MeshIndices[i]].Vertices[j].Y);//warning axe Y et Z inverse

                    verticeTmp.Add(VectTmp);
                }

                //set normal : actually doesn’t work

                /* var norm = model.Meshes[refNode.MeshIndices[i]].HasNormals ? model.Meshes[refNode.MeshIndices[i]].Normals[i] : new Vector3D();
                 var texC = model.Meshes[refNode.MeshIndices[i]].HasTextureCoords(0) ? model.Meshes[refNode.MeshIndices[i]].GetTextureCoords(0)[i] : new Vector3D();
                 var tan = model.Meshes[refNode.MeshIndices[i]].HasTangentBasis ? model.Meshes[refNode.MeshIndices[i]].Tangents[i] : new Vector3D();
                 newNode.meshes[i].normals[0].x = norm.X;
                 newNode.meshes[i].normals[0].y = norm.Y;
                 newNode.meshes[i].normals[0].z = norm.Z;*/

                //set vertices and triangle
                newNode.meshes[i] = new UnityEngine.Mesh();

                newNode.meshes[i].SetVertices(verticeTmp);
                newNode.meshes[i].triangles = model.Meshes[i].GetIndices();

                //draw the mesh
                mf.mesh = newNode.meshes[i];
            }
        }

    }

}
