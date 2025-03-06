using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MHFSM
{
    public class ParamLayer : GraphLayer
    {
        #region 字段

        private ReorderableList _reorderableList;
        private ReorderableList _reorderableListStates;
        private Vector2 _scrollView;
        
        private bool _isRenaming = false;
        private string _newName;
        private List<FSMParameterData> _emptyList = new List<FSMParameterData>();
        private Rect _headerRect = new Rect();
        private string[] _toolbars = new string[] { "Controllers", "Parameters" };
        private int _select = 1;
        private int index = 0;
        
        #endregion
        
        public ParamLayer(EditorWindow editorWindow) : base(editorWindow) { }

        public override void OnGUI(Rect rect)
        {
            rect.Set(rect.x + 2, rect.y, rect.width, rect.height);
            base.OnGUI(rect);
            _headerRect.Set(rect.x, rect.y, rect.width, 20);
            _headerRect.Set(_headerRect.x + 5, _headerRect.y, 150, _headerRect.height);
            _select = GUI.Toolbar(_headerRect, _select, _toolbars, "toolbarbuttonLeft");
            rect.Set(rect.x, rect.y + 20, rect.width, rect.height);

            if (_select == 1)
                DrawParameters(rect);
            else DrawStates(rect);
        }
        private void DrawParameters(Rect rect)
        {
            if (_reorderableList == null)
            {
                if (Context.Instance.RuntimeFSMController != null)
                    _reorderableList = new ReorderableList(Context.Instance.RuntimeFSMController.parameters, typeof(FSMParameterData), true, true, true, true);
                else
                    _reorderableList = new ReorderableList(_emptyList, typeof(FSMParameterData), true, true, true, true);
                
                _reorderableList.drawHeaderCallback += HeaderCallbackDelegate;
                _reorderableList.onAddDropdownCallback += OnAddDropdownCallback;
                _reorderableList.onRemoveCallback += OnRemoveCallback;
                _reorderableList.drawElementCallback += DrawElementCallback;
                _reorderableList.onCanRemoveCallback += OnCanRemoveCallback;
                _reorderableList.onCanAddCallback += OnCanRemoveCallback;
            }

            if (Context.Instance.RuntimeFSMController != null)
                _reorderableList.list = Context.Instance.RuntimeFSMController.parameters;
            else _reorderableList.list = _emptyList;

            EditorGUI.BeginDisabledGroup(Context.Instance.RuntimeFSMController == null);
            GUILayout.BeginArea(rect);
            _scrollView = GUILayout.BeginScrollView(_scrollView);
            _reorderableList.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            EditorGUI.EndDisabledGroup();
        }
        private void HeaderCallbackDelegate(Rect rect)
        {
            rect.width *= 0.6f;
            rect.x += 15;
            GUI.Label(rect, "名称");
            rect.x += rect.width;
            rect.width /= 3;
            rect.x -= 10;
            GUI.Label(rect, "类型");
            rect.x += rect.width;
            rect.x -= 5;
            GUI.Label(rect, "值");
        }
        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < Enum.GetValues(typeof(ParameterType)).Length; i++)
            {
                ParameterType type = (ParameterType)Enum.GetValues(typeof(ParameterType)).GetValue(i);
                menu.AddItem(new GUIContent(type.ToString()), false, () =>
                {
                    FSMParameterFactory.CreateParameter(Context.Instance.RuntimeFSMController, type);
                });
            }
            
            menu.ShowAsContext();
        }
        private void OnRemoveCallback(ReorderableList list)
        {
            FSMParameterFactory.RemoveParameter(Context.Instance.RuntimeFSMController, list.index);
        }
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Context.Instance.RuntimeFSMController == null) return;
            if (index < 0 || index >= Context.Instance.RuntimeFSMController.parameters.Count) return;
            
            FSMParameterData parameter = Context.Instance.RuntimeFSMController.parameters[index];

            if (parameter == null) return;

            rect.width *= 0.6f;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) && isFocused)
            {
                _isRenaming = true;
                this.index = index;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                !rect.Contains(Event.current.mousePosition) && index == _reorderableList.index)
            {
                EditorApplication.delayCall += () =>
                {
                    _isRenaming = false;
                };

                GUI.FocusControl(null);
            }
            
            // 按下回车键的时候，也需要取消重命名
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                EditorApplication.delayCall += () =>
                {
                    _isRenaming = false;
                };
            }

            if (_isRenaming && index == _reorderableList.index)
            {
                EditorGUI.BeginChangeCheck();
                _newName = EditorGUI.DelayedTextField(rect, parameter.name);
                if (EditorGUI.EndChangeCheck() && this.index == index)
                {
                    _isRenaming = false;
                    if (parameter.name.Equals(_newName)) return;
                    FSMParameterFactory.RenameParameter(Context.Instance.RuntimeFSMController, parameter, _newName);
                }
            }
            else
                GUI.Label(rect, parameter.name);

            rect.x += rect.width;
            rect.width /= 3;
            EditorGUI.BeginDisabledGroup(true);
            GUI.Label(rect, GetParameterType(parameter));
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;

            switch (parameter.parameterType)
            {

                case ParameterType.Float:
                    parameter.Value = EditorGUI.FloatField(rect, parameter.Value);
                    break;
                case ParameterType.Int:
                    parameter.Value = EditorGUI.IntField(rect, (int)parameter.Value);
                    break;
                case ParameterType.Bool:
                    parameter.Value = EditorGUI.Toggle(rect, parameter.Value == 1) ? 1 : 0;
                    break;
                case ParameterType.Trigger:
                    parameter.Value = EditorGUI.Toggle(rect, parameter.Value == 1, GUI.skin.GetStyle("Radio")) ? 1 : 0;
                    break;
            }
        }
        private string GetParameterType(FSMParameterData parameter)
        {
            switch (parameter.parameterType)
            {
                case ParameterType.Float:
                    return "Float";
                case ParameterType.Int:
                    return "Int";
                case ParameterType.Bool:
                    return "Bool";
                case ParameterType.Trigger:
                    return "Trigger";
            }

            return string.Empty;
        }
        private bool OnCanRemoveCallback(ReorderableList list)
        {
            return Application.isPlaying == false;
        }
        
        private void DrawStates(Rect rect)
        {
            if (_reorderableListStates == null)
            {
                _reorderableListStates = new ReorderableList(Context.Instance.RuntimeFSMControllers, typeof(FSMParameterData), false, true, false, false);
                _reorderableListStates.headerHeight = 0;
                
                _reorderableListStates.drawElementCallback += DrawStateElementCallback;
                _reorderableListStates.onSelectCallback += OnStateChanged;
            }

            for (int i = Context.Instance.RuntimeFSMControllers.Count - 1; i >= 0; i--)
            {
                if (Context.Instance.RuntimeFSMControllers[i] == null)
                    Context.Instance.RuntimeFSMControllers.RemoveAt(i);
            }
            
            _reorderableListStates.list = Context.Instance.RuntimeFSMControllers;
            
            GUILayout.BeginArea(rect);
            _scrollView = GUILayout.BeginScrollView(_scrollView);
            _reorderableListStates.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        private void DrawStateElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= Context.Instance.RuntimeFSMControllers.Count) return;

            RuntimeFSMController controller = Context.Instance.RuntimeFSMControllers[index];

            try
            {
                GUIContent content = EditorGUIUtility.IconContent("AnimatorController Icon");

                if (controller == Context.Instance.RuntimeFSMController)
                    GUI.Label(new Rect(rect.x, rect.y, rect.height, rect.height), "✓");

                GUI.Label(new Rect(rect.x + rect.height, rect.y, rect.height, rect.height), content);
                rect.Set(rect.x + rect.height * 2, rect.y, rect.width - rect.height, rect.height);
                GUI.Label(rect, controller == null ? "None" : controller.name);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        private void OnStateChanged(ReorderableList list)
        {
            if (Context.Instance.RuntimeFSMControllers == null || Context.Instance.RuntimeFSMControllers.Count == 0) return;

            if (Context.Instance.RuntimeFSMController == Context.Instance.RuntimeFSMControllers[list.index]) return;

            Context.Instance.FSMControllerIndex = list.index;
        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition))
                Context.Instance.ClearSelections();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            _isRenaming = false;
        }
    }
}
