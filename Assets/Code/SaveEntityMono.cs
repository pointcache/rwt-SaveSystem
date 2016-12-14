using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

public class NotEditableStringAttribute : PropertyAttribute { }
/// <summary>
/// This component should not be added in code on runtime. Only objects that are Instanced and already have it.
/// Thus: only add in editor to the prefab.
/// </summary>
public class SaveEntityMono : MonoBehaviour
{
    [NotEditableString] 
    public string ID; 
    [NotEditableString]
    public string prefab;

    
    public void PostLoad(SaveEntity entity)
    {

    }

    public SaveEntity GetData()
    {
        SaveEntity entity = new SaveEntity();
        entity.prefab = prefab;
        entity.position = transform.position;
        entity.rotation = transform.rotation.eulerAngles;
        entity.data = CollectData();
        return entity;
        
    }

    
    List<ISerializedData> CollectData()
    {
        List<ISerializedData> data = new List<ISerializedData>();
        var interfaces = GetInterfacesInStack<ISerializedDataMono>();
        foreach (var i in interfaces)
        {
            data.Add(i.get_data());
        }

        return data;
    }

    public string ConvertToSaved()
    {
        if (string.IsNullOrEmpty(ID))
            ID = Guid.NewGuid().ToString();
        SaveSystem.Register(ID, GetInstanceID());
        return ID;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor Only
    /// </summary>
    void Reset()
    {
        if (perform_editor_validity_check())
        {
            UnityEngine.Object obj = PrefabUtility.GetPrefabParent(gameObject);
            if (obj == null)
                obj = gameObject;

            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.Contains("Resources/"))
            {
                Debug.LogError("<color=red>Prefab is not in RESOURCES, this is forbidden.</color>");
                prefab = "ERROR=" + path;
                return;
            }

            path = path.Replace("Assets/", "").Replace("Resources/", "").Replace(".prefab", "");
            prefab = path;

        }
    }

    bool perform_editor_validity_check()
    {
        if (!CheckIfPrefabObject(gameObject))
        {
            Debug.LogError("You can only add this component to a prefab's root.");
            DestroyImmediate(this);
            return false;
        }
        if (transform.parent != null)
        {
            Debug.LogError("You can only add this component to a prefab's root.");
            DestroyImmediate(this);
            return false;
        }

        Debug.Log("<color=green>Success.</color>");
        return true;
    }
    static bool CheckIfPrefabObject(GameObject go)
    {
        return PrefabUtility.GetPrefabParent(go) == null && PrefabUtility.GetPrefabObject(go) != null;
    }

    static bool CheckIfHasPrefab(GameObject go)
    {
        return PrefabUtility.GetPrefabParent(go) != null;
    }
#endif


    List<T> GetInterfacesInStack<T>() where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return null;
        }
        List<T> list = new List<T>();
        list.AddRange(GetComponents<Component>().OfType<T>().ToList());

        foreach (Transform t in transform)
        {
            getinterfacesInStackRecursive(t.gameObject, list);
        }
        return list;
    }

    static void getinterfacesInStackRecursive<T>(GameObject inObj, List<T> list) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return;
        }

        list.AddRange(inObj.GetComponents<Component>().OfType<T>().ToList());

        foreach (Transform t in inObj.transform)
        {
            getinterfacesInStackRecursive(t.gameObject, list);
        }
    }
}
