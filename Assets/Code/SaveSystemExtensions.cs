using UnityEngine;
using System.Collections;

public static class SaveSystemExtensions
{
    public static SaveEntity GetSaveEntity(this MonoBehaviour mb)
    {
        SaveEntityMono smono = mb.GetComponent<SaveEntityMono>();
        if (!smono)
        {
            return getsaveentity_recursive(mb.transform);
        }
        else
            return smono.entity;
    }

    static SaveEntity getsaveentity_recursive(this Transform tr)
    {

        SaveEntityMono smono = tr.GetComponent<SaveEntityMono>();
        if (!smono)
        {
            if (tr.parent == null)
            {
                Debug.LogError("SaveEntityMono not found.", tr.gameObject);
                return null;
            }
            else
            {
               return getsaveentity_recursive(tr.parent);
            }
        }
        else
            return smono.entity;
    }
}