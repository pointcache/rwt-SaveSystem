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

    const string DATA_BUILD_PATH = "/GameData/worldData.json";

    public Config config;
    [Serializable]
    public class Config
    {

    }

    public List<SceneData> levels = new List<SceneData>();

    public TextAsset save_file;

    SaveData _currentSave; //always assume its null
    public SaveData currentSave
    {
       get {
            if (_currentSave == null)
                GetSaveData();
            return _currentSave;

        }
    }

    public static Dictionary<string, int> idMapping = new Dictionary<string, int>();

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
        LoadSceneEditor();

    }

    internal static string GetUID()
    {
        throw new NotImplementedException();
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
        List<SaveEntity> lvdata = null;
        savedata.levelsdata.TryGetValue(scene, out lvdata);
        if (lvdata == null)
        {
            lvdata = new List<SaveEntity>();
            savedata.levelsdata.Add(scene, lvdata);
        }

        lvdata.Clear();
        lvdata.AddRange(CollectLevelEntitiesData());
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
        List<SaveEntity> lvdata = null;
        savedata.levelsdata.TryGetValue(scene, out lvdata);
        if (lvdata == null)
        {
            lvdata = new List<SaveEntity>();
            savedata.levelsdata.Add(scene, lvdata);
        }

        lvdata.Clear();
        lvdata.AddRange(CollectLevelEntitiesDataEditor());
        SaveDataBuildDefault();
    }

#if UNITY_EDITOR
    static List<SaveEntity> CollectLevelEntitiesDataEditor()
    {
        List<SaveEntity> list = new List<SaveEntity>();

        SaveEntityMono[] entities = GameObject.FindObjectsOfType<SaveEntityMono>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
            SaveEntity data = entities[i].GetData();
            list.Add(data);
            DestroyImmediate(entities[i].gameObject);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        return list;
    } 
#endif

    static List<SaveEntity> CollectLevelEntitiesData()
    {
        List<SaveEntity> list = new List<SaveEntity>();

        SaveEntityMono[] entities = GameObject.FindObjectsOfType<SaveEntityMono>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
            SaveEntity data = entities[i].GetData();
            list.Add(data);
        }
        return list;
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

    public static void Unregister(string id)
    {
        idMapping.Remove(id);
    }

    public static void Register(string id, int value)
    {
        if (!idMapping.ContainsKey(id))
            idMapping.Add(id, value);
    }

    public static int GetInstanceId(string id)
    {
        return idMapping[id];
    }

    static void save_to_disk(string path, SaveData data)
    {
        SerializationHelper.Serialize(data, path, true);
    }

    static SaveData load_from_disk(string path)
    {
        return SerializationHelper.Load<SaveData>(path);
    }

    static SaveData load_from_file(TextAsset asset)
    {
        return SerializationHelper.LoadFromString<SaveData>(asset.text);
    }

    void rebuild_scene_from_save(SaveData data)
    {
        string scene = SceneManager.GetActiveScene().name;

        List<SaveEntity> list = null;
        data.levelsdata.TryGetValue(scene, out list);

        if (list == null)
        {
            Debug.LogError("No save data for this scene found.");
            return;
        }
#if UNITY_EDITOR
        foreach (var entity in list)
        {
            GameObject pref = Resources.Load(entity.prefab) as GameObject;
            GameObject go = PrefabUtility.InstantiatePrefab(pref) as GameObject;

            go.transform.position = entity.position;
            go.transform.rotation = Quaternion.Euler(entity.rotation);
        }
#else
        foreach (var entity in list)
        {
            GameObject pref = Resources.Load(entity.prefab) as GameObject;
            GameObject go = Instantiate(pref);

            go.transform.position = entity.position;
            go.transform.rotation = Quaternion.Euler(entity.rotation);
        }
#endif

    }
}
