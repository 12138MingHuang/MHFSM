using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MHFSM
{
    public class FSMSelectParamWindow : PopupWindowContent
    {
        private float _width;
        private FSMConditionData _condition;
        private RuntimeFSMController _controller;

        private SearchField _searchField;
        private Rect _searchRect;
        private const float SearchHeight = 25f;
        
        private Rect _labelRect;
        private const float LabelHeight = 30f;

        private FSMParamListTree _paramTree;
        private TreeViewState _paramState;
        private Rect _paramRect;
        
        public FSMSelectParamWindow(float width, FSMConditionData condition, RuntimeFSMController controller)
        {
            _width = width;
            _condition = condition;
            _controller = controller;
        }
        
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, 120);
        }

        public override void OnGUI(Rect rect)
        {
            if (_paramTree == null)
            {
                if (_paramState == null)
                    _paramState = new TreeViewState();
                
                _paramTree = new FSMParamListTree(_paramState, _controller, _condition);
                _paramTree.Reload();
            }

            if (_searchField == null)
                _searchField = new SearchField();

            _searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 10, SearchHeight);
            _paramTree.searchString = _searchField.OnGUI(_searchRect, _paramTree.searchString);
            
            _labelRect.Set(rect.x, rect.y + SearchHeight, rect.width, LabelHeight);
            EditorGUI.LabelField(_labelRect, _condition.parameterName, GUI.skin.GetStyle("AC BoldHeader"));

            _paramRect.Set(rect.x, rect.y + SearchHeight + LabelHeight - 5, rect.width, rect.height - SearchHeight - LabelHeight);
            _paramTree.OnGUI(_paramRect);
        }
    }
}
