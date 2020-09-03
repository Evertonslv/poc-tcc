using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class InformationObject
{
    public string Name;
    public string Path;
    public int IdMeker;
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
}

[Serializable]
public class InformationObjectList
{
    public List<InformationObject> ListInformationObject;

    public InformationObjectList()
    {
        ListInformationObject = new List<InformationObject>();
    }
}