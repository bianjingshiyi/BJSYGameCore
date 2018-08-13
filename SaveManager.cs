using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

using TBSGameCore;

namespace TBSGameCore
{
    public class SaveManager : MonoBehaviour
    {
        [Header("实体")]
        [SerializeField]
        List<SavableInstanceRegistration> _refs = new List<SavableInstanceRegistration>();
        [SerializeField]
        List<int> _IDPool = new List<int>();
        public int allocate(SavableInstance instance)
        {
            int id = 0;
            if (_IDPool.Count > 0)
            {
                id = _IDPool[_IDPool.Count - 1];
                _IDPool.RemoveAt(_IDPool.Count - 1);
            }
            else
            {
                id = _refs.Count + 1;
            }
            _refs.Add(new SavableInstanceRegistration(id, instance));
            return id;
        }
        protected void allocate(SavableInstance instance, int id)
        {
            _refs.Add(new SavableInstanceRegistration(id, instance));
        }
        public void reallocate(int id, SavableInstance instance)
        {
            SavableInstanceRegistration r = _refs.Find(e => { return e.id == id; });
            if (r != null)
                r.instance = instance;
            else
                Debug.LogWarning("重新分配ID失败！" + id + "没有被分配。", instance);
        }
        public void deallocateAt(int index)
        {
            _IDPool.Add(_refs[index].id);
            _refs.RemoveAt(index);
        }
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
            SaveData data = new SaveData();
            data.name = name;
            data.date = DateTime.Now;
            data.savedObjects = new List<ILoadableData>((this).findInstances<ISavable>().Select(e => { return e.save(); }));
            return data;
        }
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
        Dictionary<int, SavableInstanceData> _loadingObjects = null;
        /// <summary>
        /// 加载存档数据。
        /// </summary>
        /// <param name="data"></param>
        private void load(SaveData data)
        {
            //先加载对所有SavableInstance的引用。
            _loadingObjects = new Dictionary<int, SavableInstanceData>();
            foreach (ILoadableData d in data.savedObjects)
            {
                if (d is SavableInstanceData)
                {
                    _loadingObjects.Add((d as SavableInstanceData).id, d as SavableInstanceData);
                }
            }
            //再逐个读取。
            foreach (ILoadableData d in data.savedObjects)
            {
                loadInstance(d, gameObject.scene);
            }
            _loadingObjects = null;
        }
        private ISavable loadInstance(ILoadableData d, Scene scene)
        {
            ISavable s = d.load(scene);
            if (d is SavableInstanceData && s is SavableInstance)
            {
                allocate(s as SavableInstance, (d as SavableInstanceData).id);
            }
            return s;
        }
        public T getInstanceById<T>(int id, bool intcludeNotLoaded = false) where T : SavableInstance
        {
            SavableInstanceRegistration reference = _refs.Find(e => { return e.id == id; });
            if (reference != null)
                return reference.instance as T;
            else if (intcludeNotLoaded && _loadingObjects != null && _loadingObjects.ContainsKey(id))
            {
                return loadInstance(_loadingObjects[id], gameObject.scene) as T;
            }
            else
                return null;
        }
        protected void OnDrawGizmos()
        {
            for (int i = 0; i < _refs.Count; i++)
            {
                if (_refs[i].instance == null)
                {
                    deallocateAt(i);
                    i--;
                }
            }
        }
        [ContextMenu("全部重新注册")]
        private void re_RegisterAll()
        {
            SavableInstance[] instances = this.findInstances<SavableInstance>();
            _refs = new List<SavableInstanceRegistration>(instances.Length);
            _IDPool = new List<int>();
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].data.id = allocate(instances[i]);
            }
        }
    }
}