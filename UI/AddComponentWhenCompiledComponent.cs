using UnityEngine;

namespace BJSYGameCore.UI
{
    public class AddComponentWhenCompiledComponent : MonoBehaviour
    {
        [SerializeField]
        string _path;
        public string path
        {
            get { return _path; }
            set { _path = value; }
        }
    }
}