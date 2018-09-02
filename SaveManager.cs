using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

using TBSGameCore;

namespace TBSGameCore
{
    [ExecuteInEditMode]
    public class SaveManager : MonoBehaviour
    {
        #region Save
        public void saveAsFile(string path)
        {
            FileInfo file = new FileInfo(path);
            string fileName = file.Name.Split('.')[0];
            SaveData data = save(fileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, data);
            }
        }
        public byte[] saveAsBytes(string name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, save(name));
                byte[] bytes = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(bytes, 0, (int)ms.Length);
                return bytes;
            }
        }
        private SaveData save(string name)
        {
            SaveData data = new SaveData
            {
                name = name,
                date = DateTime.Now,
                instances = new List<SavableInstanceData>(),
                savedObjects = new List<SaveObjectData>()
            };
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                findAndSaveObject(data, roots[i].transform, null, roots[i].name, null);
            }
            return data;
        }
        private void findAndSaveObject(SaveData data, Transform transform, SavableInstance current, string path, string relative)
        {
            ISavable[] objs = transform.GetComponents<ISavable>();
            for (int i = 0; i < objs.Length; i++)
            {
                LoadPriorityAttribute att = objs[i].GetType().GetCustomAttribute<LoadPriorityAttribute>();
                data.savedObjects.Add(new SaveObjectData(current != null ? current.id : 0, current != null ? relative : path, att == null ? 0 : att.priority, objs[i].save()));
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                findAndSaveObject(data, child, current, path + '/' + child.gameObject.name, relative == null ? child.gameObject.name : (relative + '/' + child.gameObject.name));
            }
        }
        #endregion
        #region Load
        public void loadFromFile(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
                    if (data != null)
                    {
                        load(data);
                    }
                }
            }
        }
        public void loadFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                SaveData data = bf.Deserialize(ms) as SaveData;
                if (data != null)
                {
                    load(data);
                }
            }
        }
        SaveData loadingData
        {
            get; set;
        }
        public SaveObjectData[] loadingObjects
        {
            get { return loadingData.savedObjects.ToArray(); }
        }
        /// <summary>
        /// 加载存档数据。
        /// </summary>
        /// <param name="data"></param>
        private void load(SaveData data)
        {
            //按照优先级排序
            loadingData = data;
            loadingData.savedObjects.Sort((a, b) =>
            {
                if (a.priority > b.priority)
                    return 1;
                else if (a.priority < b.priority)
                    return -1;
                else
                    return 0;
            });
            //再加载
            for (int i = 0; i < loadingData.savedObjects.Count; i++)
            {
                loadInstance(loadingData.savedObjects[i], gameObject.scene);
            }
        }
        private ISavable loadInstance(SaveObjectData obj, Scene scene)
        {
            ISavable s = obj.data.load(this, obj.id, obj.path);
            return s;
        }
        #endregion
    }
}