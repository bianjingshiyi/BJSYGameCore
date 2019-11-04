
using UnityEditor;
using UnityEngine;

using BJSYGameCore.SaveSystem;

namespace BJSYGameCore
{
    public class DrawerDebugger : EditorWindow
    {
        [MenuItem("Window/DrawerDebugger")]
        public static void open()
        {
            GetWindow<DrawerDebugger>("DrawerDebugger");
        }
        protected void OnGUI()
        {
        }
    }
}