using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

using BJSYGameCore;

namespace BJSYGameCore
{
    [ExecuteInEditMode]
    public class SaveManager : MonoBehaviour
    {
        #region Save
        public void saveAsFile(string path, object header = null)
        {
            FileInfo file = new FileInfo(path);
            string fileName = file.Name.Split('.')[0];
            SaveData data = save(fileName);
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                onSaveAsFile(writer.BaseStream);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                //文件头
                if (header != null)
                {
                    byte[] headerBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        binaryFormatter.Serialize(ms, header);
                        ms.Position = 0;
                        headerBytes = new byte[ms.Length];
                        ms.Read(headerBytes, 0, (int)ms.Length);
                    }
                    writer.Write(BitConverter.GetBytes(headerBytes.Length));
                    writer.Write(headerBytes);
                }
                else
                    writer.Write(0);
                //主体
                binaryFormatter.Serialize(writer.BaseStream, data);
            }
        }
        protected virtual void onSaveAsFile(Stream stream)
        {
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
                data.savedObjects.Add(new SaveObjectData(current != null ? current.id : 0, current != null ? relative : path, getTypePriority(objs[i].GetType()), objs[i].save()));
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                findAndSaveObject(data, child, current, path + '/' + child.gameObject.name, relative == null ? child.gameObject.name : (relative + '/' + child.gameObject.name));
            }
        }
        private float getTypePriority(Type type)
        {
            LoadPriorityAttribute att = type.GetCustomAttribute<LoadPriorityAttribute>();
            if (att != null)
                return att.priority;
            LoadBeforeAttribute beforeAtt = type.GetCustomAttribute<LoadBeforeAttribute>();
            if (beforeAtt != null)
                return getTypePriority(beforeAtt.targetType) - 1;
            LoadAfterAttribute afterAtt = type.GetCustomAttribute<LoadAfterAttribute>();
            if (afterAtt != null)
                return getTypePriority(afterAtt.targetType) + 1;
            return 0;
        }
        #endregion
        #region Load
        /// <summary>
        /// 读取存档文件的文件头
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件头对象</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SerializationException"></exception>
        public object loadHeaderFromFile(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    onLoadFromFile(stream);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    try
                    {
                        //文件头
                        byte[] buffer = new byte[4];
                        stream.Read(buffer, 0, 4);
                        int headerLength = BitConverter.ToInt32(buffer, 0);
                        object header;
                        if (headerLength > 0)
                        {
                            buffer = new byte[headerLength];
                            using (MemoryStream ms = new MemoryStream(stream.Read(buffer, 4, headerLength)))
                            {
                                header = binaryFormatter.Deserialize(ms);
                            }
                        }
                        else
                            header = null;
                        return header;
                    }
                    catch (SerializationException e)
                    {
                        throw e;
                    }
                }
            }
            else
                throw new FileNotFoundException("没有找到存档文件" + path, path);
        }
        /// <summary>
        /// 从文件中加载存档
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <exception cref="FileNotFoundException">当无法找到路径上的文件时抛出</exception>
        /// <exception cref="SerializationException">当存档内容为空或者反序列化失败的时候抛出</exception>
        public object loadFromFile(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    onLoadFromFile(stream);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    try
                    {
                        //文件头
                        byte[] buffer = new byte[4];
                        stream.Read(buffer, 0, 4);
                        int headerLength = BitConverter.ToInt32(buffer, 0);
                        object header;
                        if (headerLength > 0)
                        {
                            buffer = new byte[headerLength];
                            using (MemoryStream ms = new MemoryStream(stream.Read(buffer, 4, headerLength)))
                            {
                                header = binaryFormatter.Deserialize(ms);
                            }
                        }
                        else
                            header = null;
                        //主体
                        if (binaryFormatter.Deserialize(stream) is SaveData data)
                            load(data);
                        return header;
                    }
                    catch (SerializationException e)
                    {
                        throw e;
                    }
                }
            }
            else
                throw new FileNotFoundException("没有找到存档文件" + path, path);
        }
        protected virtual void onLoadFromFile(Stream stream)
        {
        }
        public void loadFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                if (bf.Deserialize(ms) is SaveData data)
                    load(data);
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