using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;

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
            if (targets.Length == 1)
            {
                Animator animator = (target as UIObject).GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController is AnimatorController controller)
                {
                    AnimatorControllerLayer removeLayer = null;
                    foreach (AnimatorControllerLayer layer in controller.layers)
                    {
                        Match m = Regex.Match(layer.name, @"(?<name>\w+)Controller");
                        if (m.Success)
                        {
                            string controllerName = m.Groups["name"].Value;
                            if (!controllerFoldDic.ContainsKey(controllerName))
                                controllerFoldDic.Add(controllerName, false);
                            EditorGUILayout.BeginHorizontal();
                            //展开Controller
                            controllerFoldDic[controllerName] = EditorGUILayout.Foldout(controllerFoldDic[controllerName], controllerName);
                            if (EditorApplication.isPlaying && targets.Length == 1)
                            {
                                //Controller当前状态
                                EditorGUILayout.LabelField((target as UIObject).getController(controllerName, layer.stateMachine.states.Select(s => s.state.name).ToArray()), GUILayout.Width(100));
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
                                        string saveDirPath = EditorUtility.SaveFolderPanel("保存新建AnimationClip", Application.dataPath, "Animations")
                                            .Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", null);
                                        if (!string.IsNullOrEmpty(saveDirPath))
                                        {
                                            AnimationClip newClip = new AnimationClip();
                                            AssetDatabase.CreateAsset(newClip, saveDirPath + "/" + controllerName + "_" + controllerNewStateDic[controllerName] + ".anim");
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                            AnimatorState newState = new AnimatorState()
                                            {
                                                name = controllerNewStateDic[controllerName],
                                                motion = newClip
                                            };
                                            layer.stateMachine.AddState(newState, new Vector3(250, layer.stateMachine.states.Length * 50));
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                    //删除Controller的实际处理
                    if (removeLayer != null)
                        controller.RemoveLayer(Array.FindIndex(controller.layers, l => l.name == removeLayer.name));
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
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
                EditorGUILayout.LabelField("无法同时编辑多个物体的动画");
            base.OnInspectorGUI();//Generated fields
        }
    }
}