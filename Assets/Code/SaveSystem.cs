using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.Callbacks;
using FullSerializer;
using System.IO;
#if UNITY_EDITOR
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

    [HideInInspector]
    public SaveData currentSave;

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

    }

    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        //deduct if we already built this level

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

    public static void ToSavedState()
    {
        SaveSystem sys = instance;

        if (sys.currentSave == null)
            sys.currentSave = new SaveData();
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
        SaveDataBuildDefault();
    }

    static List<SaveEntity> CollectLevelEntitiesData()
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

        return list;

    }

    public static void ToLoadedState()
    {
        SaveSystem sys = instance;

        if (sys.save_file == null)
        {
            sys.currentSave = load_from_disk(Application.dataPath + DATA_BUILD_PATH);
            if (sys.currentSave == null)
            {
                Debug.LogError("No save file at >" + Application.dataPath + DATA_BUILD_PATH + " was found, place it manually in the slot.");
                return;
            }
        }
        else
        {
            sys.currentSave = load_from_file(sys.save_file);
        }

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
