using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BJSYGameCore
{
    public static class EditorHelper
    {
        /// <summary>
        /// 尝试选择文件路径并将它保存为字段，下一次选择路径会优先打开上一次选择的文件夹。
        /// </summary>
        /// <param name="pathField">路径字段</param>
        /// <param name="title"></param>
        /// <param name="defaultName"></param>
        /// <param name="extension">不需要带.</param>
        /// <param name="defaultDir">如果不填，那么是Application.streamingAssetsPath</param>
        /// <returns></returns>
        public static bool trySelectFilePath(ref string pathField, string title, string defaultName, string extension, string defaultDir = null, bool isOpenFile = false)
        {
            string dir = File.Exists(pathField) ? Path.GetDirectoryName(pathField) : (Directory.Exists(defaultDir) ? defaultDir : Application.streamingAssetsPath);
            if (isOpenFile)
                pathField = EditorUtility.OpenFilePanel(title, dir, extension);
            else
                pathField = EditorUtility.SaveFilePanel(title, dir, defaultName, extension);
            if (Directory.Exists(Path.GetDirectoryName(pathField)))
                return true;
            else
                return false;
        }
        public static MonoScript FindMonoScriptByType(Type type)
        {
            var scripts = AssetDatabase.FindAssets(type.Name + " t:" + typeof(MonoScript).Name)
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                .Where(s => s != null);
            foreach (var script in scripts)
            {
                if (script.GetClass() == type)
                    return script;
            }
            return null;
        }
        public static bool tryFindScript(Func<MonoScript, bool> filter, out MonoScript targetScript)
        {
            var scripts = AssetDatabase.FindAssets("t:" + typeof(MonoScript).Name)
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                .Where(s => s != null);
            foreach (MonoScript script in scripts)
            {
                if (!filter(script))
                    continue;
                targetScript = script;
                return true;
            }
            targetScript = null;
            return false;
        }
    }
}