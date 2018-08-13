using UnityEngine;

namespace TBSGameCore
{
    public abstract class SavableEntity : MonoBehaviour, ISavable
    {
        public abstract SavableEntityData data
        {
            get;
        }
        public int id
        {
            get { return data.id; }
            private set
            {
                data.id = value;
            }
        }
        public SavableEntityReference reference
        {
            get { return new SavableEntityReference(id); }
        }
        public ILoadableData save()
        {
            data.path = gameObject.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                data.path = parent.gameObject.name + '/' + data.path;
            }
            return data;
        }
#if UNITY_EDITOR
        bool _checked = false;
#endif
        protected void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (id == 0)
            {
                //没有注册，注册ID。
                Game saveManager = this.findInstance<Game>();
                if (saveManager == null)
                {
                    saveManager = new GameObject("SaveManager").AddComponent<Game>();
                }
                id = saveManager.allocate(this);
                _checked = true;
            }
            else if (!_checked)
            {
                //已经注册，没有检查，检查是否实际上丢失了注册。
                Game saveManager = this.findInstance<Game>();
                if (saveManager == null)
                {
                    saveManager = new GameObject("SaveManager").AddComponent<Game>();
                }
                SavableEntity other = saveManager.getInstanceById<SavableEntity>(id);
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
#endif
        }
    }
}