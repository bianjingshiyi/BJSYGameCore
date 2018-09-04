using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore
{
    [Serializable]
    public class ObjectPool
    {
        public GameObject create(out int ID)
        {
            return create(null, Vector3.zero, Quaternion.identity, out ID);
        }
        public GameObject create(Transform parent, out int ID)
        {
            return create(parent, Vector3.zero, Quaternion.identity, out ID);
        }
        public GameObject create(Vector3 position, Quaternion rotation, out int ID)
        {
            return create(position, rotation, out ID);
        }
        public GameObject create(Transform parent, Vector3 position, Quaternion rotation, out int ID)
        {
            GameObject instance;
            if (_pool.Count > 0)
            {
                ObjectPoolPair p = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                _instances.Add(p);
                instance = p.instance;
                ID = p.id;
                //设置实例状态
                instance.transform.parent = parent;
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.SetActive(true);
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(_prefab, position, rotation, parent);
                _instances.Add(new ObjectPoolPair() { id = _instances.Count + 1, instance = instance });
                ID = _instances.Count;
            }
            return instance;
        }
        public void addAsInstance(int id, GameObject instance)
        {
            _instances.Add(new ObjectPoolPair() { id = id, instance = instance });
        }
        public int getIDOfInstance(GameObject instance)
        {
            foreach (var p in _instances)
            {
                if (p.instance == instance)
                    return p.id;
            }
            return 0;
        }
        public GameObject getInstanceByID(int ID)
        {
            var p = _instances.Find(e => { return e.id == ID; });
            if (p != null)
                return p.instance;
            else
                return null;
        }
        public GameObject[] getAllInstance()
        {
            return _instances.Select(e => { return e.instance; }).ToArray();
        }
        [SerializeField]
        List<ObjectPoolPair> _instances;
        public bool destroy(GameObject instance)
        {
            var p = _instances.Find(e => { return e.instance == instance; });
            if (p != null)
            {
                p.instance.SetActive(false);
                _instances.Remove(p);
                _pool.Add(p);
                return true;
            }
            else
                return false;
        }
        [SerializeField]
        List<ObjectPoolPair> _pool;
        [SerializeField]
        GameObject _prefab;
        [Serializable]
        class ObjectPoolPair
        {
            public int id;
            public GameObject instance;
        }
    }
}