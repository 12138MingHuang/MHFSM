using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MHFSM
{
    public class FSMParamListTree : TreeView
    {
        private RuntimeFSMController _controller;
        private FSMConditionData _condition;


        public FSMParamListTree(TreeViewState state, RuntimeFSMController controller, FSMConditionData condition) : base(state)
        {
            _controller = controller;
            _condition = condition;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }
        
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1);

            if (_controller != null)
            {
                for (int i = 0; i < _controller.parameters.Count; i++)
                {
                    root.AddChild(new TreeViewItem(i, 0, _controller.parameters[i].name));
                }
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

            string parameterName = FindItem(id, rootItem).displayName;

            FSMParameterData param = _controller.GetParameterData(parameterName);

            if (param != null)
            {
                _condition.parameterName = parameterName;

                switch (param.parameterType)
                {

                    case ParameterType.Float:
                    case ParameterType.Int:
                        _condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        _condition.compareType = CompareType.Equal;
                        break;
                    case ParameterType.Trigger:
                        _condition.compareType = CompareType.Equal;
                        _condition.targetValue = 1;
                        break;
                }
            }
            else
            {
                Debug.LogErrorFormat($"参数查询失败:{parameterName}");
            }
            
            _controller.Save();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            if (args.label.Equals(_condition.parameterName))
                GUI.Label(args.rowRect, "√");
        }
    }
}
