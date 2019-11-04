using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    [Serializable]
    public class ScenePath
    {
        public Scene scene
        {
            get { return SceneManager.GetSceneByPath(_value); }
        }
        public string value
        {
            get { return _value; }
        }
        [SerializeField]
        string _value;
        public AsyncOperation loadScene(LoadSceneMode loadMode)
        {
            return SceneManager.LoadSceneAsync(value, loadMode);
        }
        public AsyncOperation unloadScene()
        {
            return SceneManager.UnloadSceneAsync(value);
        }
        public static implicit operator string(ScenePath scene)
        {
            return scene.value;
        }
    }
}