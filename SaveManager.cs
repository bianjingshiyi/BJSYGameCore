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
    [ExecuteInEditMode]
    public class SaveManager : MonoBehaviour
    {
        #region Instance
        [Header("实体")]
        [SerializeField]
        List<SavableInstanceRegistration> _instances = new List<SavableInstanceRegistration>();
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
                id = _instances.Count + 1;
            }
            _instances.Add(new SavableInstanceRegistration(id, instance));
            return id;
        }
        protected void allocate(SavableInstance instance, int id)
        {
            _instances.Add(new SavableInstanceRegistration(id, instance));
        }
        public void reallocate(int id, SavableInstance instance)
        {
            SavableInstanceRegistration r = _instances.Find(e => { return e.id == id; });
            if (r != null)
                r.instance = instance;
            else
                Debug.LogWarning("重新分配ID失败！" + id + "没有被分配。", instance);
        }
        public void deallocateAt(int index)
        {
            _IDPool.Add(_instances[index].id);
            _instances.RemoveAt(index);
        }
        public SavableInstance getInstanceById(int id)
        {
            SavableInstanceRegistration reference = _instances.Find(e => { return e.id == id; });
            if (reference != null)
            {
                return reference.instance;
            }
            else
            {
                return null;
            }
        }
        public T getInstanceById<T>(int id, bool intcludeNotLoaded = false) where T : SavableInstance
        {
            SavableInstanceRegistration reference = _instances.Find(e => { return e.id == id; });
            if (reference != null)
                return reference.instance as T;
            else if (intcludeNotLoaded && _loadingObjects != null && _loadingObjects.ContainsKey(id))
            {
                return loadInstance(_loadingObjects[id], gameObject.scene) as T;
            }
            else
                return null;
        }
        #endregion
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
            SaveData data = new SaveData();
            data.name = name;
            data.date = DateTime.Now;
            data.instances = new List<SavableInstanceData>(this.findInstances<SavableInstance>().Select(e => { return new SavableInstanceData() { id = e.id, path = e.path }; }));
            data.savedData = new List<ILoadableData>((this).findInstances<ISavable>().Select(e => { return e.save(); }));
            return data;
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
        Dictionary<int, SavableInstanceData> _loadingObjects = null;
        /// <summary>
        /// 加载存档数据。
        /// </summary>
        /// <param name="data"></param>
        private void load(SaveData data)
        {
            //先加载对所有SavableInstance。
            for (int i = 0; i < data.instances.Count; i++)
            {
                SavableInstanceRegistration registration = _instances.FirstOrDefault(e => { return e.id == data.instances[i].id; });
                if (registration != null)
                {

                }
                else
                {
                    SavableInstance.create(data.instances[i].id, gameObject.scene, data.instances[i].path);
                }
            }
            _loadingObjects = new Dictionary<int, SavableInstanceData>();
            foreach (ILoadableData d in data.savedData)
            {
                if (d is SavableInstanceData)
                {
                    _loadingObjects.Add((d as SavableInstanceData).id, d as SavableInstanceData);
                }
            }
            //再逐个读取。
            foreach (ILoadableData d in data.savedData)
            {
                loadInstance(d, gameObject.scene);
            }
            _loadingObjects = null;
        }
        private SavableInstance loadInstance(SavableInstanceData d, Scene scene)
        {
            SavableInstance instance = SavableInstance.create(d.id, scene, d.path);
            allocate(instance, instance.id);
            return instance;
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
        #endregion
        protected void Update()
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i].instance == null)
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
            _instances = new List<SavableInstanceRegistration>(instances.Length);
            _IDPool = new List<int>();
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].id = allocate(instances[i]);
            }
        }
    }
}