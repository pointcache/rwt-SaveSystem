using UnityEngine;
using System;
using System.Collections.Generic;

public class DontDestroyOnLoad : MonoBehaviour {

	void OnEnable()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}
