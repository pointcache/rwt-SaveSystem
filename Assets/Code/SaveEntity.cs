using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveEntity 
{
    public string ID;
    public Vector3 position;
    public Vector3 rotation;
    public string prefab;

    public List<ISerializedData> data;
}
