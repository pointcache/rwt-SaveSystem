using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour {

    public List<SceneData> levels = new List<SceneData>();

    void OnEnable()
    {

    }

	public string GetScene(SceneData level)
    {
        return levels.FirstOrDefault(x => x == level).scene;
    }

    public SceneData GetCurrentLevelData()
    {
        var scene = SceneManager.GetActiveScene();
        return levels.FirstOrDefault(x => x.scene == scene.name);
    }
}
