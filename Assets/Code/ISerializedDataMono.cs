using UnityEngine;
using System;
using System.Collections.Generic;

public interface ISerializedDataMono  {


    ISerializedData get_data();
    //Use Instead of OnEnable
    void PostLoad();
}
