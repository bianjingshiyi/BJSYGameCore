
using UnityEditor;
using UnityEngine;

namespace TBSGameCore
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
            TriggerFuncDrawer.disable = GUILayout.Toggle(TriggerFuncDrawer.disable, "TriggerFunc");
            TriggerActionDrawer.disable = GUILayout.Toggle(TriggerActionDrawer.disable, "TriggerAction");
        }
    }
}