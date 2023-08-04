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
            get { return UnityEngine.SceneManagement.SceneManager.GetSceneByPath(_value); }
        }
        public string value
        {
            get { return _value; }
        }
        [SerializeField]
        string _value;
        public AsyncOperation loadScene(LoadSceneMode loadMode)
        {
            return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(value, loadMode);
        }
        public AsyncOperation unloadScene()
        {
            return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(value);
        }
        public static implicit operator string(ScenePath scene)
        {
            return scene.value;
        }
    }
}