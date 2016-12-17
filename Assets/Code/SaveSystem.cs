using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

public class SaveSystem : MonoBehaviour
{
    #region SINGLETON
    private static SaveSystem _instance;
    public static SaveSystem instance
    {
        get
        {
            if (!_instance) _instance = GameObject.FindObjectOfType<SaveSystem>();
            return _instance;
        }
    }
    #endregion

    const string DATA_BUILD_PATH = "/GameData/worldData.json";
    public List<SceneData> levels = new List<SceneData>();
    public TextAsset save_file;
    SaveData _currentSave = null; //always assume its null

    public SaveData currentSave
    {
        get
        {
            if (_currentSave == null || _currentSave.levelsdata.Count == 0)
                GetSaveData();
            return _currentSave;

        }
    }

    public static void ClearSceneEditor()
    {
        SaveEntityMono[] entities = GameObject.FindObjectsOfType<SaveEntityMono>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
             Undo.DestroyObjectImmediate(entities[i].gameObject);
        }
    }

    public Config config;
    [Serializable]
    public class Config
    {

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += LevelInitialization;
    }

    SaveData GetSaveData()
    {
        if (save_file == null)
        {
            _currentSave = load_from_disk(Application.dataPath + DATA_BUILD_PATH);
            if (_currentSave == null)
            {
                Debug.LogError("No save file at >" + Application.dataPath + DATA_BUILD_PATH + " was found, place it manually in the slot.");
                return null;
            }
        }
        else
        {
            _currentSave = load_from_file(save_file);
        }
        return _currentSave;
    }

    public static void SwitchScene(string scene)
    {
        SaveScene();
        SceneManager.LoadScene(scene);
    }

    void LevelInitialization(Scene scene, LoadSceneMode mode)
    {
        LoadScene();

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

    public static void SaveScene()
    {
        SaveSystem sys = instance;

        SaveData savedata = sys.currentSave;

        string scene = SceneManager.GetActiveScene().name;
        Dictionary<string, SaveEntity> lvdata = null;
        savedata.levelsdata.TryGetValue(scene, out lvdata);
        if (lvdata == null)
        {
            lvdata = new Dictionary<string, SaveEntity>();
            savedata.levelsdata.Add(scene, lvdata);
        }

        CollectLevelEntitiesData(lvdata);
    }

    public static void LoadScene()
    {
        if (GameObject.FindObjectOfType<SaveEntityMono>())
        {
            Debug.LogError("Loading of save data if SaveEntities are present in scene is forbidden as it may duplicate amount of entities.");
            return;
        }

        SaveSystem sys = instance;
        sys.rebuild_scene_from_save(sys.currentSave);
    }

    public static void SaveSceneEditor()
    {
        SaveSystem sys = instance;

        SaveData savedata = sys.currentSave;

        string scene = SceneManager.GetActiveScene().name;
        Dictionary<string, SaveEntity> lvdata = null;
        savedata.levelsdata.TryGetValue(scene, out lvdata);
        if (lvdata == null)
        {
            lvdata = new Dictionary<string, SaveEntity>();
            savedata.levelsdata.Add(scene, lvdata);
        }


        CollectLevelEntitiesDataEditor(lvdata);
        SaveDataBuildDefault();
    }

#if UNITY_EDITOR
    static void CollectLevelEntitiesDataEditor(Dictionary<string, SaveEntity> dict)
    {
        dict.Clear();
        SaveEntityMono[] entities = GameObject.FindObjectsOfType<SaveEntityMono>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
            SaveEntity data = entities[i].SerializeAndGetData();
            if (String.IsNullOrEmpty(data.ID))
            {
                data.ID = get_unique_id(dict);
            }

            dict.Add(data.ID, data);
            DestroyImmediate(entities[i].gameObject);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
#endif

    static void CollectLevelEntitiesData(Dictionary<string, SaveEntity> dict)
    {
        dict.Clear();
        SaveEntityMono[] entities = GameObject.FindObjectsOfType<SaveEntityMono>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
            SaveEntity data = entities[i].SerializeAndGetData();
            dict.Add(data.ID, data);
        }
    }

    public static void LoadSceneEditor()
    {
        if (GameObject.FindObjectOfType<SaveEntityMono>())
        {
            Debug.LogError("Loading of save data if SaveEntities are present in scene is forbidden as it may duplicate amount of entities.");
            return;
        }

        SaveSystem sys = instance;
        sys.rebuild_scene_from_save(sys.currentSave);
    }

    public static void SaveDataBuildDefault()
    {
        save_to_disk(Application.dataPath + DATA_BUILD_PATH, instance.currentSave);
    }

    static void save_to_disk(string path, SaveData data)
    {
        SerializationHelper.Serialize(data, path, true);
    }

    static SaveData load_from_disk(string path)
    {
        SaveData data = SerializationHelper.Load<SaveData>(path);;

        return data;
    }

    static SaveData load_from_file(TextAsset asset)
    {
        return SerializationHelper.LoadFromString<SaveData>(asset.text);
    }

    void rebuild_scene_from_save(SaveData data)
    {
        string scene = SceneManager.GetActiveScene().name;

        Dictionary<string, SaveEntity> dict = null;
        data.levelsdata.TryGetValue(scene, out dict);

        if (dict == null)
        {
            Debug.LogError("No save data for this scene found.");
            return;
        }
#if UNITY_EDITOR
        foreach (var pair in dict)
        {
            var entity = pair.Value;

            GameObject pref = Resources.Load(entity.prefab) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(pref) as GameObject;

            go.transform.position = entity.position;
            go.transform.rotation = Quaternion.Euler(entity.rotation);

            go.GetComponent<SaveEntityMono>().PostLoad(entity);
        }
#else
        foreach (var pair in dict)
        {
            var entity = pair.Value;

            GameObject pref = Resources.Load(entity.prefab) as GameObject;
            GameObject go = Instantiate(pref);

            go.transform.position = entity.position;
            go.transform.rotation = Quaternion.Euler(entity.rotation);
            
            go.GetComponent<SaveEntityMono>().PostLoad(entity);
        }
#endif

    }

    static string get_unique_id(Dictionary<string, SaveEntity> dict)
    {
        Guid guid = Guid.NewGuid();
        string id = guid.ToString();
        if (dict.ContainsKey(id))
            return get_unique_id(dict);
        else
            return id;
    }

}
