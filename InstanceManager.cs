using System;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore
{
    [LoadPriority(-64)]
    [ExecuteInEditMode]
    public class InstanceManager : MonoBehaviour, ISavable
    {
        protected void Update()
        {
            cleanNullRegistrations();
        }
        [ContextMenu("全部重新注册")]
        private void re_RegisterAll()
        {
            SavableInstance[] instances = this.findInstances<SavableInstance>();
            _registrations = new List<SavableInstanceRegistration>(instances.Length);
            _data.idPool = new List<int>();
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].id = allocate(instances[i]);
            }
        }
        public int allocate(SavableInstance instance)
        {
            int id = 0;
            if (_data.idPool.Count > 0)
            {
                id = _data.idPool[_data.idPool.Count - 1];
                _data.idPool.RemoveAt(_data.idPool.Count - 1);
            }
            else
                id = _registrations.Count + 1;
            _registrations.Add(new SavableInstanceRegistration(id, instance));
            return id;
        }
        protected void allocate(SavableInstance instance, int id)
        {
            _registrations.Add(new SavableInstanceRegistration(id, instance));
        }
        public void reallocate(int id, SavableInstance instance)
        {
            SavableInstanceRegistration r = _registrations.Find(e => { return e.id == id; });
            if (r != null)
                r.instance = instance;
            else
                Debug.LogWarning("重新分配ID失败！" + id + "没有被分配。", instance);
        }
        public SavableInstance getInstanceById(int id)
        {
            SavableInstanceRegistration reference = _registrations.Find(e => { return e.id == id; });
            if (reference != null)
                return reference.instance;
            else
                return null;
        }
        public void dellocate(SavableInstance instance)
        {
            if (instance != null)
            {
                _data.idPool.Add(instance.id);
                int index = _registrations.FindIndex(e => { return e.id == instance.id; });
                if (index > 0)
                    _registrations.RemoveAt(index);
            }
        }
        void deallocateAt(int index)
        {
            _data.idPool.Add(_registrations[index].id);
            _registrations.RemoveAt(index);
        }
        private void cleanNullRegistrations()
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                if (_registrations[i].instance == null)
                {
                    deallocateAt(i);
                    i--;
                }
            }
        }
        private void clearAllInstance()
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                Destroy(_registrations[i].instance.gameObject);
            }
        }
        public ILoadableData save()
        {
            cleanNullRegistrations();
            //保存实例
            _data.instances = new SavableInstanceData[_registrations.Count];
            for (int i = 0; i < _data.instances.Length; i++)
            {
                SavableInstance instance = _registrations[i].instance;
                _data.instances[i] = new SavableInstanceData(instance.id, instance.path);
            }
            return _data;
        }
        [SerializeField]
        InstanceManagerData _data = new InstanceManagerData();
        [Serializable]
        class InstanceManagerData : ILoadableData
        {
            public List<int> idPool = new List<int>();
            public SavableInstanceData[] instances = null;
            public ISavable load(SaveManager saveManager, int id, string path)
            {
                InstanceManager manager = saveManager.findInstance<InstanceManager>();
                //先清理所有实例
                manager.clearAllInstance();
                //再创建被保存的实例
                if (instances != null)
                {
                    for (int i = 0; i < instances.Length; i++)
                    {
                        SavableInstance instance = SavableInstance.create(saveManager.gameObject.scene, instances[i].path, instances[i].id);
                        manager._registrations.Add(new SavableInstanceRegistration(instance.id, instance));
                    }
                }
                return manager;
            }
        }
        [SerializeField]
        List<SavableInstanceRegistration> _registrations = new List<SavableInstanceRegistration>();
    }
}