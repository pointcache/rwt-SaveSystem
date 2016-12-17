using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData  {
    
    public Dictionary<string, Dictionary<string, SaveEntity>> levelsdata
        = new Dictionary<string, Dictionary<string,SaveEntity>>();
    
}
