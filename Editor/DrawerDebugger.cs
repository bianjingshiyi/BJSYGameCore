
using UnityEditor;
using UnityEngine;

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
            InstanceReferenceDrawer.disable = GUILayout.Toggle(InstanceReferenceDrawer.disable, nameof(InstanceReferenceDrawer));
            TriggerFuncDrawer.disable = GUILayout.Toggle(TriggerFuncDrawer.disable, "TriggerFunc");
            TriggerActionDrawer.disable = GUILayout.Toggle(TriggerActionDrawer.disable, "TriggerAction");
        }
    }
}