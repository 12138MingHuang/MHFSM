using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MHFSM
{
    public class FSMStateTreeViewItem : TreeViewItem
    {
        public MonoScript MonoScript { get; private set; }

        private Type _type = null;
        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = MonoScript.GetClass();

                return _type;
            }
        }

        public FSMStateTreeViewItem(int id, int depth, string displayName, MonoScript monoScript) : base(id, depth, displayName)
        {
            MonoScript = monoScript;
        }
    }

    public class FSMStateListTree : TreeView
    {
        private RuntimeFSMController _controller;
        private GUIStyle _style = new GUIStyle("label");
        private FSMStateNodeData _nodeData = null;
        private FSMSelectStateWindow _editorWindow = null;
        private List<int> _selections = new List<int>();

        public FSMStateListTree(TreeViewState state, RuntimeFSMController controller, FSMStateNodeData nodeData,FSMSelectStateWindow editorWindow) : base(state)
        {
            _controller = controller;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
            _nodeData = nodeData;
            _editorWindow = editorWindow;
        }
        
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1);

            List<MonoScript> scripts = AssemblyTool.GetAllStatesType();

            for (int i = 0; i < scripts.Count; i++)
            {

                Type type = scripts[i].GetClass();

                if (type == null) continue;
                string displayName = type.Name;
                FSMStateTreeViewItem item = new FSMStateTreeViewItem(i, 0, displayName, scripts[i]);
                root.AddChild(item);
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            FSMStateTreeViewItem item = FindItem(id, rootItem) as FSMStateTreeViewItem;

            if (item != null)
            {
                try
                {
                    _nodeData.AddStateScript(item.MonoScript);
                }
                catch (Exception e)
                {
                    _editorWindow.editorWindow.ShowNotification(new GUIContent(e.Message));
                    return;
                }
            }
            // 保存
            _controller.Save();
            _editorWindow.Close();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            FSMStateTreeViewItem item = args.item as FSMStateTreeViewItem;
            if (item == null) return;
            Type type = item.Type;
            if (type == null) return;

            if (args.rowRect.Contains(Event.current.mousePosition))
            {
                _selections.Clear();
                _selections.Add(item.id);
                SetSelection(_selections);
            }

            Rect rect = new Rect();
            rect.Set(args.rowRect.x + 20, args.rowRect.y, args.rowRect.width - 20, args.rowRect.height);

            GUI.Label(new Rect(0, args.rowRect.y - 2, 20, 20), EditorGUIUtility.IconContent("d_cs Script Icon"));

            if (string.IsNullOrEmpty(type.Namespace))
            {
                GUI.Label(rect, type.Name);
            }
            else
            {
                _style.richText = true;
                GUI.Label(rect, $"{type.Name}<color=#A4A4A4>({type.Namespace})</color>", _style);
            }
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
    }
}
