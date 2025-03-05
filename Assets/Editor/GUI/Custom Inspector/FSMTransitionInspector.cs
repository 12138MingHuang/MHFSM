using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MHFSM
{
    [CustomEditor(typeof(FSMTransitionInspectorHelper))]
    public class FSMTransitionInspector : Editor
    {
        private ReorderableList _reorderableList;

        private Rect _conditionLeftRect;
        private Rect _conditionRightRect;
        private Rect _popRect;

        private GUIContent _addAsetsOfConditions = null;
        private GUIContent _autoSwitchContent = null;
        
        private static Dictionary<ParameterType, FSMConditionInspector> _conditionInspectorDict = new Dictionary<ParameterType, FSMConditionInspector>();
        private Dictionary<int, ReorderableList> _reorderableDict = new Dictionary<int, ReorderableList>();

        private bool _autoSwitch;

        private void OnEnable()
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;

            if (helper == null) return;
            
            _reorderableList = new ReorderableList(helper.transitionData.conditions, typeof(FSMCondition), true, true, true, true);
            _reorderableList.drawHeaderCallback += OnDrawHeaderCallback;
            _reorderableList.onAddCallback += OnAddCallback;
            _reorderableList.onRemoveCallback += OnRemoveCallback;
            _reorderableList.drawElementCallback += DrawItem;
            
            _autoSwitch = helper.transitionData.autoSwitch;
        }
        private void OnDrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect,"Conditions");
        }
        private void OnAddCallback(ReorderableList list)
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            FSMConditionFactory.CreateCondition(helper.controller, helper.transitionData);
        }
        private void OnRemoveCallback(ReorderableList list)
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            FSMConditionFactory.DeleteCondition(helper.controller, helper.transitionData, list.index);
        }
        private void DrawItem(Rect rect, int index, bool isactive, bool isfocused)
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            FSMConditionData conditionData = helper.transitionData.conditions[index];
            
            DrawItemExecute(rect, conditionData);
        }
        private void DrawItemExecute(Rect rect, FSMConditionData conditionData)
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;

            _conditionLeftRect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            _conditionRightRect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (helper.controller.parameters.Count > 0)
            {
                if (EditorGUI.DropdownButton(_conditionLeftRect, new GUIContent(conditionData.parameterName), FocusType.Keyboard))
                {
                    _popRect.Set(rect.x, rect.y + 2, rect.width / 2, rect.height);
                    // TODO: 弹出窗口选择参数
                    // PopupWindow.Show(_popRect, new);
                }
            }

            InitConditionInspectors();
            
            FSMParameterData parameterData = helper.controller.GetParameterData(conditionData.parameterName);

            if (parameterData == null)
                EditorGUI.LabelField(_conditionRightRect, "缺少参数");
            else
            {
                // 根据不同参数类型绘制不同内容
                if (_conditionInspectorDict.ContainsKey(parameterData.parameterType))
                {
                    if (_conditionInspectorDict[parameterData.parameterType] != null)
                        _conditionInspectorDict[parameterData.parameterType].OnGUI(_conditionRightRect, conditionData, helper.controller);
                    else
                        Debug.LogErrorFormat($"未查询到对应的绘制方式:{parameterData.parameterType}");
                }
            }
        }
        private static void InitConditionInspectors()
        {
            if (_conditionInspectorDict.Count == 0)
            {
                _conditionInspectorDict.Add(ParameterType.Bool, new FSMBoolConditionInspector());
                _conditionInspectorDict.Add(ParameterType.Float, new FSMFloatConditionInspector());
                _conditionInspectorDict.Add(ParameterType.Int, new FSMIntConditionInspector());
                _conditionInspectorDict.Add(ParameterType.Trigger, new FSMConditionInspector()); // 不需要做任何绘制 , 直接显示参数名即可
            }
        }

        public override void OnInspectorGUI()
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (_autoSwitchContent == null)
                _autoSwitchContent = new GUIContent("AutoSwitch", "当前条件为空时,s是否自动切换?当前过渡条件为空时可用");

            EditorGUI.BeginDisabledGroup(!helper.transitionData.Empty);
            Rect rect = GUILayoutUtility.GetRect(0f, 20, GUILayout.ExpandWidth(expand: true));
            GUI.Label(rect, _autoSwitchContent);
            rect.Set(rect.width - 20, rect.y, 20, 20);
            if (helper.transitionData.Empty)
                helper.transitionData.autoSwitch = GUI.Toggle(rect, helper.transitionData.autoSwitch, string.Empty);
            else GUI.Toggle(rect, false, string.Empty);

            if (_autoSwitch != helper.transitionData.autoSwitch)
            {
                helper.controller.Save();
                _autoSwitch = helper.transitionData.autoSwitch;
            }
            GUILayout.Space(10);
            EditorGUI.EndDisabledGroup();

            _reorderableList.list = helper.transitionData.conditions;
            _reorderableList.DoLayoutList();
            
            GUILayout.Space(10);

            for (int i = 0; i < helper.transitionData.groupConditions.Count; i++)
            {
                GroupCondition condition = helper.transitionData.groupConditions[i];
                if (condition == null) continue;
                
                ReorderableList reorderableList = GetReorderableList(condition);

                if (reorderableList != null)
                {
                    reorderableList.list = condition.conditions;
                    reorderableList.DoLayoutList();
                    GUILayout.Space(10);
                }
            }

            if (_addAsetsOfConditions == null)
                _addAsetsOfConditions = new GUIContent("ADD a set of conditions", "添加一组条件");
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (helper.transitionData.groupConditions.Count > 0)
                GUILayout.Label("注:当有多组条件时,其中一组满足,状态就会切换!", "CN StatusWarn");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_addAsetsOfConditions, GUILayout.Width(260), GUILayout.Height(25)))
            {
                helper.transitionData.groupConditions.Add(new GroupCondition());
                helper.controller.Save();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUI.EndDisabledGroup();
        }
        private ReorderableList GetReorderableList(GroupCondition condition)
        {
            if (condition == null) return null;
            
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return null;
            
            int key = condition.GetHashCode();
            if (!_reorderableDict.ContainsKey(key))
            {
                ReorderableList reorderableList = new ReorderableList(condition.conditions, typeof(FSMConditionData), true, true, true, true);
                reorderableList.drawHeaderCallback += (rect) =>
                {
                    GUI.Label(rect, "Conditions");
                    rect.Set(rect.width - 10, rect.y + 1, 25, 25);
                    if (GUI.Button(rect, EditorGUIUtility.IconContent("d_Menu"), "IconButton"))
                        ShowConditionMenu(condition);
                };
                reorderableList.onAddCallback += (list) =>
                {
                    FSMConditionData conditionData = FSMConditionFactory.CreateCondition(helper.controller);
                    condition.conditions.Add(conditionData);
                    helper.controller.Save();
                };
                reorderableList.onRemoveCallback += (list) =>
                {
                    if (list.index >= 0 && list.index < condition.conditions.Count)
                        condition.conditions.RemoveAt(list.index);
                    helper.controller.Save();
                };
                reorderableList.drawElementCallback += (rect, index, c, d) =>
                {
                    DrawItemExecute(rect, condition.conditions[index]);
                };
                _reorderableDict.Add(key, reorderableList);
            }
            
            return _reorderableDict[key];
        }
        private void ShowConditionMenu(GroupCondition condition)
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                helper.transitionData.groupConditions.Remove(condition);
                helper.controller.Save();
            });
            
            menu.ShowAsContext();
        }

        protected override void OnHeaderGUI()
        {
            FSMTransitionInspectorHelper helper = (FSMTransitionInspectorHelper)target;
            if (helper == null) return;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstatetransition icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.Label($"{helper.transitionData.fromStateName} -> {helper.transitionData.toStateName}");
            GUILayout.EndHorizontal();
            
            // 一条分割线
            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}
