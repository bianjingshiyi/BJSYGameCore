using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
namespace BJSYGameCore.AutoCompo
{
    public class AssetDatabaseHelper
    {
        public static IEnumerable<T> getAssetsOfType<T>() where T : Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name).Select(guid =>
                  AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }
    }
}