using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine.UIElements;

namespace BJSYGameCore.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIObject), true)]
    class UIObjectEditor : Editor
    {
        Dictionary<string, bool> controllerFoldDic { get; } = new Dictionary<string, bool>();
        Dictionary<string, string> controllerNewStateDic { get; } = new Dictionary<string, string>();
        string newControllerName;
        public override void OnInspectorGUI()
        {
            if (target is UIObject obj && !obj.useController)
            {
                if (targets.Length == 1)
                {
                    Animator animator = (target as UIObject).GetComponent<Animator>();
                    if (animator == null || animator.runtimeAnimatorController == null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("必须有Animator和AnimatorController才能使用Controller");
                        if (GUILayout.Button("创建Controller"))
                        {
                            if (animator == null)
                                animator = (target as UIObject).gameObject.AddComponent<Animator>();
                            string controllerPath = EditorUtility.SaveFilePanel("保存动画控制器", Application.dataPath, (target as UIObject).gameObject.name + "_Controller", "controller");
                            controllerPath = controllerPath.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", string.Empty);
                            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                            animator.runtimeAnimatorController = controller;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (animator.runtimeAnimatorController is AnimatorController controller)
                    {
                        AnimatorControllerLayer removeLayer = null;
                        AnimatorControllerLayer rebuildLayer = null;
                        string controllerPath = AssetDatabase.GetAssetPath(controller);
                        string controllerDir = Path.GetDirectoryName(controllerPath);
                        foreach (AnimatorControllerLayer layer in controller.layers)
                        {
                            Match m = Regex.Match(layer.name, @"(?<name>\w+)Controller");
                            if (m.Success)
                            {
                                string controllerName = m.Groups["name"].Value;
                                if (!controllerFoldDic.ContainsKey(controllerName))
                                    controllerFoldDic.Add(controllerName, false);
                                layer.defaultWeight = 1;
                                EditorGUILayout.BeginHorizontal();
                                //展开Controller
                                controllerFoldDic[controllerName] = EditorGUILayout.Foldout(controllerFoldDic[controllerName], controllerName);
                                if (EditorApplication.isPlaying && targets.Length == 1)
                                {
                                    //Controller当前状态
                                    string currentStateName = (target as UIObject).getController(controllerName, layer.stateMachine.states.Select(s => s.state.name).ToArray());
                                    EditorGUILayout.LabelField(currentStateName, GUILayout.Width(100));
                                    Debug.Log(currentStateName, target);
                                }
                                else if (layer.stateMachine != null && layer.stateMachine.states.Length > 0)
                                {
                                    SerializedProperty initStatesProp = serializedObject.FindProperty("_initStates");
                                    int propIndex = -1;
                                    for (int i = 0; i < initStatesProp.arraySize; i++)
                                    {
                                        if (initStatesProp.GetArrayElementAtIndex(i).stringValue.Contains(controllerName))
                                        {
                                            propIndex = i;
                                            break;
                                        }
                                    }
                                    if (propIndex < 0)
                                    {
                                        initStatesProp.arraySize++;
                                        propIndex = initStatesProp.arraySize - 1;
                                    }
                                    string initState = initStatesProp.GetArrayElementAtIndex(propIndex).stringValue.Replace(controllerName + "/", null);
                                    int stateIndex = Array.FindIndex(layer.stateMachine.states, s => s.state.name == initState);
                                    if (stateIndex < 0)
                                        stateIndex = 0;
                                    int newStateIndex = EditorGUILayout.Popup(stateIndex, layer.stateMachine.states.Select(s => s.state.name).ToArray(), GUILayout.Width(100));
                                    if (newStateIndex != stateIndex)
                                    {
                                        animator.enabled = true;
                                        animator.speed = 0;
                                        animator.Play(layer.stateMachine.states[newStateIndex].state.name, animator.GetLayerIndex(controllerName + "Controller"));
                                        animator.Update(Time.deltaTime);
                                    }
                                    initStatesProp.GetArrayElementAtIndex(propIndex).stringValue = controllerName + "/" + layer.stateMachine.states[newStateIndex].state.name;
                                }
                                //删除Controller
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                    removeLayer = layer;
                                EditorGUILayout.EndHorizontal();
                                //Controller内容绘制
                                if (controllerFoldDic[controllerName])
                                {
                                    EditorGUI.indentLevel++;
                                    AnimatorState removeState = null;
                                    //状态绘制
                                    if (layer.stateMachine == null)
                                    {
                                        rebuildLayer = layer;
                                    }
                                    else
                                    {
                                        foreach (ChildAnimatorState state in layer.stateMachine.states)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField(state.state.name, "");
                                            if (EditorApplication.isPlaying && GUILayout.Button(">", GUILayout.Width(20)))
                                            {
                                                (target as UIObject).setController(controllerName, state.state.name);
                                            }
                                            if (GUILayout.Button("-", GUILayout.Width(20)))
                                                removeState = state.state;
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        if (removeState != null)
                                        {
                                            if (removeState.motion is AnimationClip removeClip)
                                                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(removeClip));
                                            layer.stateMachine.RemoveState(removeState);
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                        //新建状态
                                        if (!controllerNewStateDic.ContainsKey(controllerName))
                                            controllerNewStateDic.Add(controllerName, null);
                                        controllerNewStateDic[controllerName] = EditorGUILayout.TextField(controllerNewStateDic[controllerName]);
                                        if (GUILayout.Button("AddState"))
                                        {
                                            if (layer.stateMachine.states.Any(s => s.state.name == controllerNewStateDic[controllerName]))
                                                Debug.LogError(controllerName + "中已经存在同名的状态，无法添加" + controllerNewStateDic[controllerName]);
                                            else if (string.IsNullOrEmpty(controllerNewStateDic[controllerName]))
                                                Debug.LogError("Controller状态名称不能为空");
                                            else
                                            {
                                                if (Directory.Exists(controllerDir))
                                                {
                                                    AnimationClip newClip = new AnimationClip();
                                                    AssetDatabase.CreateAsset(newClip, controllerDir + "/" + controllerName + "_" + controllerNewStateDic[controllerName] + ".anim");
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                    AnimatorState newState = new AnimatorState()
                                                    {
                                                        name = controllerNewStateDic[controllerName],
                                                        motion = newClip
                                                    };
                                                    layer.stateMachine.AddState(newState, new Vector3(250, layer.stateMachine.states.Length * 50));

                                                    EditorUtility.SetDirty(controller);
                                                    AssetDatabase.AddObjectToAsset(newState, controller);
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
                                                }
                                                else
                                                    Debug.LogError("文件夹" + controllerDir + "不存在", target);
                                            }
                                            controllerNewStateDic.Remove(controllerName);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                        //删除Controller的实际处理
                        if (removeLayer != null)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(controller) + "/" + removeLayer.stateMachine.name);
                            controller.RemoveLayer(Array.FindIndex(controller.layers, l => l.name == removeLayer.name));
                            EditorUtility.SetDirty(controller);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
                        }
                        //stateMachine的Layer丢失了似乎重新新建它是没用的。那就只能试试看重建它了。
                        if (rebuildLayer != null)
                        {
                            string controllerName = rebuildLayer.name.Replace("Controller", string.Empty);
                            controller.RemoveLayer(Array.FindIndex(controller.layers, l => l.name == rebuildLayer.name));
                            AnimatorControllerLayer layer = new AnimatorControllerLayer()
                            {
                                name = controllerName + "Controller",
                                defaultWeight = 1,
                                stateMachine = new AnimatorStateMachine()
                                {
                                    name = controllerName + "Controller"
                                }
                            };
                            foreach (string animPath in Directory.GetFiles(controllerDir, "*.anim")
                                .Select(s => s.Replace('\\', '/').Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", string.Empty)))
                            {
                                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
                                Match m = Regex.Match(clip.name, @"(?<controller>\w+)_(?<state>\w+)");
                                if (m.Success && m.Groups["controller"].Value == controllerName)
                                {
                                    string stateName = m.Groups["state"].Value;
                                    AnimatorState newState = new AnimatorState()
                                    {
                                        name = stateName,
                                        motion = clip
                                    };
                                    layer.stateMachine.AddState(newState, new Vector3(250, layer.stateMachine.states.Length * 50));
                                }
                            }
                            controller.AddLayer(layer);
                            EditorUtility.SetDirty(controller);
                            AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
                        }
                        //新建Controller
                        EditorGUILayout.BeginHorizontal();
                        newControllerName = EditorGUILayout.TextField(newControllerName);
                        if (GUILayout.Button("AddController"))
                        {
                            if (controller.layers.Any(l => l.name == newControllerName + "Controller"))
                                Debug.LogError("已经存在同名Controller，无法添加" + newControllerName);
                            else if (string.IsNullOrEmpty(newControllerName))
                                Debug.LogError("Controller名称不能为空");
                            else
                            {
                                AnimatorControllerLayer newLayer = new AnimatorControllerLayer
                                {
                                    name = newControllerName + "Controller",
                                    defaultWeight = 1,
                                    stateMachine = new AnimatorStateMachine()
                                };
                                controller.AddLayer(newLayer);
                                EditorUtility.SetDirty(controller);
                                AssetDatabase.AddObjectToAsset(newLayer.stateMachine, controller);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
                            }
                            newControllerName = null;
                        }
                        EditorGUILayout.EndHorizontal();
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                    EditorGUILayout.LabelField("无法同时编辑多个物体的动画控制器");
            }
            base.OnInspectorGUI();//Generated fields
        }
    }
}