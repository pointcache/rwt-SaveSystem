using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {

    public string scene;

    public void LoadScene()
    {
        SaveSystem.SwitchScene(scene);
    }
}
