using UnityEngine;
using BJSYGameCore;

namespace BJSYGameCore.UI
{
    public class UIManager : Manager
    {
        public T getPanel<T>() where T : UIObject
        {
            return GetComponentInChildren<T>(true);
        }
        /// <summary>
        /// 获取指定类型的UI实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>先这么凑合着，以后再优化吧</remarks>
        /// <returns></returns>
        public T getObject<T>() where T : UIObject
        {
            return this.findInstance<T>();
        }
    }
}