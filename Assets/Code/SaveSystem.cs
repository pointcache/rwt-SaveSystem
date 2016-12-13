using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.Callbacks;
using FullSerializer;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    const string DATA_BUILD_PATH = "/GameData/worldData.json";

    public Config config;
    [Serializable]
    public class Config
    {
        public bool DevMode;
    }

    public List<SceneData> levels = new List<SceneData>();
    public SaveData currentSave = new SaveData();

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

    public static void ConvertSceneTo_SavedState()
    {
        SaveSystem sys = instance;

        if (sys.currentSave == null)
            sys.currentSave = new SaveData();
        SaveData savedata = sys.currentSave;

        string scene = SceneManager.GetActiveScene().name;
        List<SerializedEntityData> lvdata = null;
        savedata.levelsdata.TryGetValue(scene, out lvdata);
        if (lvdata == null)
        {
            lvdata = new List<SerializedEntityData>();
            savedata.levelsdata.Add(scene, lvdata);
        }

        lvdata.Clear();
        lvdata.AddRange(CollectLevelEntitiesData());
        SaveDataBuildDefault();
    }

    static List<SerializedEntityData> CollectLevelEntitiesData()
    {
        List<SerializedEntityData> list = new List<SerializedEntityData>();

        SerializedEntityComponent[] entities = GameObject.FindObjectsOfType<SerializedEntityComponent>();
        int count = entities.Length;
        for (int i = 0; i < count; i++)
        {
            SerializedEntityData data = entities[i].GetData();
            list.Add(data);
            //DestroyImmediate(entities[i].gameObject);
        }

        return list;

    }

    public static void ConvertSceneTo_LoadedState() { }

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
        Serialize(data, path, true);
    }

    static bool Serialize(object obj, string path, bool beautify)
    {
        fsSerializer _serializer = new fsSerializer();
        fsData data;
        _serializer.TrySerialize(obj, out data).AssertSuccessWithoutWarnings();
        StreamWriter sw = new StreamWriter(path);
        switch (beautify)
        {
            case true:
                sw.Write(fsJsonPrinter.PrettyJson(data));
                break;
            case false:
                sw.Write(fsJsonPrinter.CompressedJson(data));
                break;
        }

        sw.Close();
        return true;
    }
    public static object Deserialize(Type type, string serializedState)
    {
        fsSerializer _serializer = new fsSerializer();
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(serializedState);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }
}
