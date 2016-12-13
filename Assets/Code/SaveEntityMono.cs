using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;

public class NotEditableStringAttribute : PropertyAttribute { }
/// <summary>
/// This component should not be added in code on runtime. Only objects that are Instanced and already have it.
/// Thus: only add in editor to the prefab.
/// </summary>
public class SaveEntityMono : MonoBehaviour
{
    [NotEditableString] // Treat this special in the editor.
    public string ID; // A String representing our Guid
    [NotEditableString]
    public string prefab;
    void OnEnable()
    {

    }

    public SaveEntity GetData()
    {
        SaveEntity data = new SaveEntity();
        data.prefab = prefab;
        data.position = transform.position;
        data.rotation = transform.rotation.eulerAngles;

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

            path = path.Replace("Assets/Resources/", "");
            path = path.Replace(".prefab", "");
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
}
