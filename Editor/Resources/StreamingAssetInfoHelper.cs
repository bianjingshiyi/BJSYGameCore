using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BJSYGameCore
{
    public static class StreamingAssetInfoHelper
    {
        /// <summary>
        /// 更新流文件信息
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/更新文件系统保存的流文件信息", false)]
        public static StreamingAssetsInfo updateStreamingAssetsInfo()
        {
            StreamingAssetsInfo streamingAssetsInfo = Resources.LoadAll<StreamingAssetsInfo>("").FirstOrDefault();
            const string RES_PATH = "Assets/Resources";
            if (streamingAssetsInfo == null)
            {
                //已经存在则直接更新信息，若不存在则创建
                if (!Directory.Exists(RES_PATH))
                {
                    Directory.CreateDirectory(RES_PATH);
                }
                streamingAssetsInfo = ScriptableObject.CreateInstance<StreamingAssetsInfo>();
                AssetDatabase.CreateAsset(streamingAssetsInfo, Path.Combine(RES_PATH, "StreamingAssetInfo.asset"));
            }
            //更新文件信息
            streamingAssetsInfo.clearFile();
            string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(".meta"))
                    continue;//忽略meta文件
                string path = files[i].Substring(Application.streamingAssetsPath.Length + 1, files[i].Length - Application.streamingAssetsPath.Length - 1);
                streamingAssetsInfo.addFile(path);
                Debug.Log("添加流文件信息" + path, streamingAssetsInfo);
            }
            EditorUtility.SetDirty(streamingAssetsInfo);
            AssetDatabase.SaveAssets();
            return streamingAssetsInfo;
        }
    }
}