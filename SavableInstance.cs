using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    [ExecuteInEditMode]
    public class SavableInstance : MonoBehaviour
    {
        [SerializeField]
        int _id = 0;
        public int id
        {
            get { return _id; }
            internal set
            {
                _id = value;
            }
        }
        public string path
        {
            get
            {
                if (transform.parent == null)
                    return gameObject.name;
                else
                {
                    string path = gameObject.name;
                    for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                    {
                        path = parent.gameObject.name + "/" + path;
                    }
                    return path;
                }
            }
        }
        public SavableInstanceReference reference
        {
            get { return new SavableInstanceReference(id, path); }
        }
        public static SavableInstance create(int id, Scene scene, string path)
        {
            SavableInstance instance = scene.createGameObjectAtPath(path).AddComponent<SavableInstance>();
            instance._id = id;
            return instance;
        }
        bool _checked = false;
        protected void Update()
        {
            if (id <= 0)
            {
                //没有注册，注册ID。
                SaveManager saveManager = this.findInstance<SaveManager>();
                if (saveManager == null)
                {
                    saveManager = new GameObject("SaveManager").AddComponent<SaveManager>();
                }
                id = saveManager.allocate(this);
                _checked = true;
            }
            else if (!_checked)
            {
                //已经注册，没有检查，检查是否实际上丢失了注册。
                SaveManager saveManager = this.findInstance<SaveManager>();
                if (saveManager == null)
                {
                    saveManager = new GameObject("SaveManager").AddComponent<SaveManager>();
                }
                SavableInstance other = saveManager.getInstanceById<SavableInstance>(id);
                if (other == null)
                {
                    //有ID但是丢失引用，重新分配引用
                    saveManager.reallocate(id, this);
                }
                else if (other != this)
                {
                    //引用被别人占据了，重新注册
                    id = saveManager.allocate(this);
                }
            }
        }
    }
}