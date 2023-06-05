#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace DGames.Ads
{
    public static class ScriptableEditorUtils
    {
        public static void OpenOrCreateDefault<T>(string defaultName = null, string childrenPath = null,
            string parentFolderPath = null) where T : ScriptableObject
        {
            defaultName ??= typeof(T).Name;

            var asset = GetOrCreate<T>(defaultName, childrenPath, parentFolderPath);

            Selection.activeObject = asset;
        }

        public static T GetOrCreate<T>(string name = "", string childrenPath = "", string parentFolder = "")
            where T : ScriptableObject
        {
            name = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
            var appendPath = string.IsNullOrEmpty(childrenPath) ? "" : $"/{childrenPath}";
            var folder =
                (string.IsNullOrEmpty(parentFolder) ? $"Assets/Resources" : $"Assets/{parentFolder}/Resources") +
                appendPath;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            return asset;
        }
    }
}

#endif