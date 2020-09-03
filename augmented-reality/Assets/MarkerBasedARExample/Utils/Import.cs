using UnityEditor;
using System;
using UnityEngine;

public class Import
{    
    public static GameObject FBX(string path) 
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    public static void Package(string path)
    {
        AssetDatabase.ImportPackage(path, false);
    }

    public static void MoveAsset(string oldPath, string newPath)
    {
        AssetDatabase.MoveAsset(oldPath, newPath);
    }
}