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
    public abstract class Game : MonoBehaviour
    {
        [Header("实体")]
        [SerializeField]
        List<SavableEntityRegistration> _refs = new List<SavableEntityRegistration>();
        [SerializeField]
        List<int> _IDPool = new List<int>();
        public int allocate(SavableEntity instance)
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
            _refs.Add(new SavableEntityRegistration(id, instance));
            return id;
        }
        protected void allocate(SavableEntity instance, int id)
        {
            _refs.Add(new SavableEntityRegistration(id, instance));
        }
        public void reallocate(int id, SavableEntity instance)
        {
            SavableEntityRegistration r = _refs.Find(e => { return e.id == id; });
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
            GameData data = save(fileName);
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
        protected abstract GameData getGameData();
        protected abstract void setGameData(GameData gameData);
        private GameData save(string name)
        {
            GameData data = getGameData();
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
                    GameData data = binaryFormatter.Deserialize(stream) as GameData;
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
                GameData data = bf.Deserialize(ms) as GameData;
                if (data != null)
                {
                    load(data);
                }
            }
        }
        Dictionary<int, SavableEntityData> _loadingObjects = null;
        /// <summary>
        /// 加载存档数据。
        /// </summary>
        /// <param name="data"></param>
        private void load(GameData data)
        {
            //先加载对所有SavableInstance的引用。
            _loadingObjects = new Dictionary<int, SavableEntityData>();
            foreach (ILoadableData d in data.savedObjects)
            {
                if (d is SavableEntityData)
                {
                    _loadingObjects.Add((d as SavableEntityData).id, d as SavableEntityData);
                }
            }
            //再逐个读取。
            foreach (ILoadableData d in data.savedObjects)
            {
                loadInstance(d, gameObject.scene);
            }
            _loadingObjects = null;
            setGameData(data);
        }
        private ISavable loadInstance(ILoadableData d, Scene scene)
        {
            ISavable s = d.load(scene);
            if (d is SavableEntityData && s is SavableEntity)
            {
                allocate(s as SavableEntity, (d as SavableEntityData).id);
            }
            return s;
        }
        public T getInstanceById<T>(int id, bool intcludeNotLoaded = false) where T : SavableEntity
        {
            SavableEntityRegistration reference = _refs.Find(e => { return e.id == id; });
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
            SavableEntity[] instances = this.findInstances<SavableEntity>();
            _refs = new List<SavableEntityRegistration>(instances.Length);
            _IDPool = new List<int>();
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].data.id = allocate(instances[i]);
            }
        }
    }
}