using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MHFSM
{
    [CustomEditor(typeof(FSMStateInspectorHelper))]
    public class FSMStateInspector : Editor
    {
        private ReorderableList _reorderableList;

        internal static Rect popupRect;

        private GUIContent _btnAddStateScript = new GUIContent(" Add State Script");
        private GUIStyle _projectBrowserHeaderBgMiddle = null;
        private GUIStyle _ddHeaderStyle = null;
        private GUIStyle _prefixLabel  = null;
        private GUIContent _scriptGUIContent = new GUIContent();
        private Vector2 _scroll;

        private void OnEnable()
        {
            FSMStateInspectorHelper helper = (FSMStateInspectorHelper)target;
            if (helper == null) return;
        }

        public override void OnInspectorGUI()
        {
            if (_projectBrowserHeaderBgMiddle == null)
                _projectBrowserHeaderBgMiddle = new GUIStyle("AC BoldHeader");

            if (_ddHeaderStyle == null)
                _ddHeaderStyle = new GUIStyle("IconButton");
            
            if(_prefixLabel == null)
                _prefixLabel = new GUIStyle("PrefixLabel");
            
            FSMStateInspectorHelper helper = (FSMStateInspectorHelper)target;
            if (helper == null) return;

            bool disabled = EditorApplication.isPlaying || helper.node.IsAnyState || helper.node.IsAnyState || helper.node.IsUpstate;
            
            EditorGUI.BeginDisabledGroup(disabled);
            Vector2 mousePosition = Event.current.mousePosition;
            foreach (FSMStateScriptInfo scriptInfo in helper.node.StateScripts)
            {
                // 刷新一下
                if (string.IsNullOrEmpty(scriptInfo.guid) && !string.IsNullOrEmpty(scriptInfo.className))
                    helper.node.RefreshStateScripts(helper.controller);
                
                // 根据guid加载到脚本信息
                string path = AssetDatabase.GUIDToAssetPath(scriptInfo.guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if(script==null) continue;
                Type type = script.GetClass();
                if (type == null) continue;

                Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
                rect.x = 0;
                rect.width += 30;
                GUI.Box(rect, string.Empty, _projectBrowserHeaderBgMiddle);
                GUILayout.Space(-10);
                GUILayout.Label(EditorGUIUtility.IconContent("d_cs Script Icon"), GUILayout.Width(20), GUILayout.Height(20));
                string displayName = string.Empty;
                if (type.IsSubclassOf(typeof(FSMState)))
                {
                    displayName = type.Name;
                    _scriptGUIContent.tooltip = string.Empty;
                }
                else
                {
                    displayName = $"{type.Name}<color=yellow>(Script Missing)</color>";
                    _scriptGUIContent.tooltip = "脚本丢失,请检查该脚本是否基层自FSMState!";
                }

                _scriptGUIContent.text = displayName;
                
                GUILayout.Label(_scriptGUIContent, _prefixLabel, GUILayout.Height(20));
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_MoreOptions"), _ddHeaderStyle, GUILayout.Width(25), GUILayout.Height(20)))
                {
                    ShowMenu(script);
                }
                GUILayout.EndVertical();
                
                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && rect.Contains(mousePosition))
                {
                    ShowMenu(script);
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && rect.Contains(mousePosition))
                {
                    EditorGUIUtility.PingObject(script);
                    Event.current.Use();
                }
            }
            
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect tempRect = GUILayoutUtility.GetRect(_btnAddStateScript, GUI.skin.button, GUILayout.Width(260), GUILayout.Height(25));

            if (GUI.Button(tempRect, _btnAddStateScript))
            {
                popupRect = new Rect(tempRect);
                popupRect.height = 300;
                
                PopupWindow.Show(tempRect, new FSMSelectStateWindow(popupRect, helper.controller, helper.node));
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (helper.controller != null)
                helper.controller.Save();
            
            EditorGUI.EndDisabledGroup();
        }
        private void ShowMenu(MonoScript script)
        {
            FSMStateInspectorHelper helper = (FSMStateInspectorHelper)target;
            if (helper == null) return;
            
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Script"), false, () =>
            {
                helper.node.RemoveStateScript(script);
                helper.controller.Save();
            });
            menu.AddItem(new GUIContent("Edit, Script"), false, () =>
            {
                AssetDatabase.OpenAsset(script);
            });
            
            menu.ShowAsContext();
        }

        protected override void OnHeaderGUI()
        {
            FSMStateInspectorHelper helper = (FSMStateInspectorHelper)target;
            if (helper == null) return;

            string name = null;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
            EditorGUILayout.LabelField("Name:", GUILayout.Width(60));
            bool disabled = EditorApplication.isPlaying || helper.node.IsAnyState || helper.node.IsAnyState || helper.node.IsUpstate;
            
            EditorGUI.BeginDisabledGroup(disabled);
            name = EditorGUILayout.DelayedTextField(helper.node.DisplayName);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                string oldName = helper.node.name;
                bool success = FSMStateNodeFactory.Rename(helper.controller, helper.node, name);
                if (success)
                    helper.graphView.RenameState(oldName, name);
            }
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.width + rect.width, rect.y));
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}
