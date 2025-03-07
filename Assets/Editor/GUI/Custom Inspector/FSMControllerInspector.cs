using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MHFSM
{
    [CustomEditor(typeof(FSMController))]
    public class FSMControllerInspector : Editor
    {
        private FSMController _controller => target as FSMController;

        private SerializedProperty _script = null;
        private SerializedProperty _resetOnDisable = null;
        private ReorderableList _controllerList = null;
        private SerializedProperty _runtimeFSMController;
        private SerializedProperty _runtimeFSMControllerLayer;
        private GUIContent _empty;
        private bool _foldout = true;

        private void OnEnable()
        {
            if (target == null) return;
            _script = serializedObject.FindProperty("m_Script");
            _resetOnDisable = serializedObject.FindProperty("_resetOnDisable");
            _runtimeFSMController = serializedObject.FindProperty("_runtimeFSMControllers");
            _runtimeFSMControllerLayer = serializedObject.FindProperty("_runtimeFSMControllerLayers");
        }

        public override void OnInspectorGUI()
        {
            if (_controller == null) return;

            if (_runtimeFSMControllerLayer.arraySize == 0 && _runtimeFSMController.arraySize > 0)
            {
                for (int i = 0; i < _runtimeFSMController.arraySize; i++)
                {
                    SerializedProperty property = _runtimeFSMController.GetArrayElementAtIndex(i);

                    if (property == null || property.objectReferenceValue == null)
                        continue;

                    int index = _runtimeFSMControllerLayer.arraySize;
                    _runtimeFSMControllerLayer.InsertArrayElementAtIndex(index);
                    SerializedProperty layer = _runtimeFSMControllerLayer.GetArrayElementAtIndex(index);
                    SerializedProperty name = layer.FindPropertyRelative("name");
                    SerializedProperty controller = layer.FindPropertyRelative("controller");

                    name.stringValue = $"Layer {i}";
                    controller.objectReferenceValue = property.objectReferenceValue;
                }

                // 清空
                _runtimeFSMController.ClearArray();

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_script);
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();

            if (_controllerList == null)
            {
                _controllerList = new ReorderableList(serializedObject, _runtimeFSMControllerLayer);
                _controllerList.headerHeight = 0;
                _controllerList.drawElementCallback = ElementCallbackDelegate;
                _controllerList.onAddCallback = AddCallbackDelegate;

                // 添加和移除用一样的
                _controllerList.onCanAddCallback = CanAddCallbackDelegate;
                _controllerList.onCanRemoveCallback = CanAddCallbackDelegate;
            }

            _foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout, "RuntimeFSMControllers");
            if (_foldout)
                _controllerList.DoLayoutList();

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.PropertyField(_resetOnDisable);
            serializedObject.ApplyModifiedProperties();
        }
        private void ElementCallbackDelegate(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _controllerList.serializedProperty.arraySize)
                return;

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            SerializedProperty property = _controllerList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty controller = property.FindPropertyRelative("controller");

            rect.width /= 2;

            string nameValue = name.stringValue;
            EditorGUI.BeginChangeCheck();
            nameValue = EditorGUI.DelayedTextField(rect, nameValue);

            if (EditorGUI.EndChangeCheck())
            {
                if (IsContainName(nameValue))
                {
                    EditorWindow.focusedWindow.ShowNotification(new GUIContent($"名称:{nameValue}已经存在!"));
                }
                else
                {
                    name.stringValue = nameValue;
                    name.serializedObject.ApplyModifiedProperties();
                }
            }

            rect.x += rect.width;

            if (_empty == null)
                _empty = new GUIContent();

            EditorGUI.PropertyField(rect, controller, _empty);
            EditorGUI.EndDisabledGroup();
        }
        private bool IsContainName(string name)
        {
            foreach (RuntimeFSMControllerLayer controllerLayer in _controller.RuntimeFSMControllerLayerList)
            {
                if (controllerLayer.name == name) return true;
            }
            
            return false;
        }
        
        private void AddCallbackDelegate(ReorderableList list)
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex(index);
            
            SerializedProperty name = property.FindPropertyRelative("name");
            name.stringValue = GetLayerName();
            name.serializedObject.ApplyModifiedProperties();
        }
        private string GetLayerName()
        {
            int index = 0;
            string name = string.Empty;

            do
            {
                name = $"Layer {index}";
                index++;
            } while (IsContainName(name));
            
            return name;
        }
        
        private bool CanAddCallbackDelegate(ReorderableList list)
        {
            return !Application.isPlaying;
        }
    }
}
