using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore
{
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
        /// <summary>
        /// 重新分配ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instance"></param>
        public void reallocate(int id, SavableInstance instance)
        {
            SavableInstanceRegistration r = _registrations.Find(e => { return e.id == id; });
            if (r != null)
                r.instance = instance;
            else
            {
                if (data.idPool.Remove(id))
                    _registrations.Add(new SavableInstanceRegistration(id, instance));
                else
                {
                    //从来都没分配过这样的ID，那么你说有就有吧。
                    _registrations.Add(new SavableInstanceRegistration(id, instance));
                }
            }
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
        InstanceManagerData data
        {
            get { return _data; }
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
                manager._data = this;
                return manager;
            }
        }
        [SerializeField]
        List<SavableInstanceRegistration> _registrations = new List<SavableInstanceRegistration>();
    }
}