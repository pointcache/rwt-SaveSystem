using UnityEngine;
using System;
using System.Collections.Generic;

public class SaveSystemLoader : MonoBehaviour {

	
    #region SINGLETON
    private static SaveSystemLoader _instance;
    public static SaveSystemLoader instance
    {
        get
        {
            if (!_instance) _instance = GameObject.FindObjectOfType<SaveSystemLoader>();
            return _instance;
        }
    }
    #endregion



    public static SaveSystemLoader current;

    
    [SerializeField]
    private bool devmode;


    void OnEnable()
    {
        if (current)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            current = this;
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
