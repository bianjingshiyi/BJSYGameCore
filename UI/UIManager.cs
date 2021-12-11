using UnityEngine;
using BJSYGameCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
        public void pushDisplayInfo(UIDisplayInfo displayInfo)
        {
            _historyUIDisplayInfoStack.Add(displayInfo);
        }
        public UIDisplayInfo popDisplayInfo()
        {
            if (_historyUIDisplayInfoStack.Count > 0)
            {
                UIDisplayInfo displayInfo = _historyUIDisplayInfoStack[_historyUIDisplayInfoStack.Count - 1];
                _historyUIDisplayInfoStack.RemoveAt(_historyUIDisplayInfoStack.Count - 1);
                return displayInfo;
            }
            else
                return null;
        }
        /// <summary>
        /// 显示一个新的UI，如果现在有正在显示的UI，那么会保存它的返回信息。
        /// </summary>
        /// <param name="uiCtrl"></param>
        /// <param name="displayInfo"></param>
        /// <returns></returns>
        public Task displayUIWithReturn(IUICtrl uiCtrl, UIDisplayInfo displayInfo)
        {
            //如果当前有已经在显示的UI
            if (_currentDisplayUICtrl != null)
            {
                //就获取它的显示信息，并放进历史ui显示信息栈里面
                UIDisplayInfo prevUIDisplayInfo = _currentDisplayUICtrl.getCurrentDisplayInfo();
                prevUIDisplayInfo.uiCtrl = _currentDisplayUICtrl;
                _historyUIDisplayInfoStack.Add(prevUIDisplayInfo);
            }
            _currentDisplayUICtrl = uiCtrl;
            return uiCtrl.display(displayInfo);
        }
        /// <summary>
        /// 返回之前显示的UI。
        /// </summary>
        /// <param name="onNothingToReturn"></param>
        /// <returns></returns>
        public async Task returnPrevUI(Action onNothingToReturn = null)
        {
            //隐藏当前显示UI
            if (_currentDisplayUICtrl != null)
            {
                await _currentDisplayUICtrl.hide();
                _currentDisplayUICtrl = null;
            }
            UIDisplayInfo displayInfo = popDisplayInfo();
            if (displayInfo != null)
            {
                //返回上层
                _currentDisplayUICtrl = displayInfo.uiCtrl;
                await displayInfo.uiCtrl.display(displayInfo);
            }
            else
            {
                //没有可以返回的UI了
                onNothingToReturn?.Invoke();
            }
        }
        /// <summary>
        /// 当前显示的UI的控制器
        /// </summary>
        IUICtrl _currentDisplayUICtrl;
        /// <summary>
        /// 之前显示的UI信息栈
        /// </summary>
        [SerializeField]
        List<UIDisplayInfo> _historyUIDisplayInfoStack = new List<UIDisplayInfo>();
    }
    public abstract class UIDisplayInfo
    {
        public IUICtrl uiCtrl;
    }
    public interface IUICtrl
    {
        Task display(UIDisplayInfo displayInfo);
        UIDisplayInfo getCurrentDisplayInfo();
        Task hide();
    }
}