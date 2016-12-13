using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData : UnityEngine.Object {

    public Dictionary<string, List<SerializedEntityData>> levelsdata = new Dictionary<string, List<SerializedEntityData>>();

}
