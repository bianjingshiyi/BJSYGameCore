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
                findAndSaveObject(data, roots[i].transform, null, "", "");
            }
            return data;
        }
        private void findAndSaveObject(SaveData data, Transform transform, SavableInstance current, string path, string relative)
        {
            SavableInstance instance = transform.GetComponent<SavableInstance>();
            if (instance != null)
            {
                data.instances.Add(new SavableInstanceData() { id = instance.id, path = path });
                current = instance;
                relative = "";
            }
            ISavable[] objs = transform.GetComponents<ISavable>();
            for (int i = 0; i < objs.Length; i++)
            {
                data.savedObjects.Add(new SaveObjectData() { id = current != null ? current.id : 0, path = relative, data = objs[i].save() });
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                findAndSaveObject(data, child, current, path + '/' + child.gameObject.name, relative + '/' + child.gameObject.name);
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
        /// <summary>
        /// 加载存档数据。
        /// </summary>
        /// <param name="data"></param>
        private void load(SaveData data)
        {
            //先加载对所有SavableInstance。
            for (int i = 0; i < data.instances.Count; i++)
            {
                SavableInstance instance = SavableInstance.create(data.instances[i].id, gameObject.scene, data.instances[i].path);
                allocate(instance, data.instances[i].id);
            }
            //再加载别的。
            for (int i = 0; i < data.savedObjects.Count; i++)
            {
                loadInstance(data.savedObjects[i], gameObject.scene);
            }
        }
        private ISavable loadInstance(SaveObjectData obj, Scene scene)
        {
            ISavable s = obj.data.load(scene, obj.id, obj.path);
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