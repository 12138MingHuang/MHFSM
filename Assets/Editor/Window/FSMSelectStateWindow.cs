using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MHFSM
{
    public class FSMSelectStateWindow : PopupWindowContent
    {
        private RuntimeFSMController _controller;

        // 搜索框
        private SearchField _searchField;
        private Rect _searchRect;
        private const float SearchHeight = 25f;

        // 标签 
        private Rect _labelRect;
        private const float LabelHeight = 30f;

        // 参数列表
        private FSMStateListTree _stateTree;
        private TreeViewState _treeViewState;
        private Rect _stateRect;

        private Rect _rect;

        private FSMStateNodeData _nodeData;

        private bool _showCreateScriptGUI = false;
        private string _scriptName = string.Empty;
        private string _scriptName2;

        public FSMSelectStateWindow(Rect rect,RuntimeFSMController controller,FSMStateNodeData nodeData)
        {
            _controller = controller;
            _rect = rect;
            _nodeData = nodeData;
            _showCreateScriptGUI = false; 
        }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(this._rect.width, this._rect.height);
        }

        public override void OnGUI(Rect rect)
        {
            if (!_showCreateScriptGUI)
            {
                OnGUISearchScripts(rect);
            }
            else {
                OnGUICreateScripts(rect);
            }
            
        }

        private void OnGUISearchScripts(Rect rect) 
        {
            if (_stateTree == null)
            {
                if (_treeViewState == null)
                {
                    _treeViewState = new TreeViewState();
                }

                _stateTree = new FSMStateListTree(_treeViewState, _controller, this._nodeData,this);
                _stateTree.Reload();
            }

            // 搜索框
            if (_searchField == null)
            {
                _searchField = new SearchField();
            }
            _searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 5, SearchHeight);
            _stateTree.searchString = _searchField.OnGUI(_searchRect, _stateTree.searchString);

            // 标签 
            _labelRect.Set(rect.x, rect.y + SearchHeight, rect.width, LabelHeight);
            EditorGUI.LabelField(_labelRect, "FSMStates", GUI.skin.GetStyle("AC BoldHeader"));

            // 参数列表 

            _stateRect.Set(rect.x, rect.y + SearchHeight + LabelHeight - 5, rect.width, rect.height - SearchHeight - LabelHeight - 20);
            _stateTree.OnGUI(_stateRect);

            _stateRect.Set(rect.x, _stateRect.y + _stateRect.height, rect.width, 23);
            if (GUI.Button(_stateRect, "New Scripts", "AppToolbarButtonMid")) {
                _showCreateScriptGUI = true;
                _scriptName = string.Empty;
            }
        }

        private void OnGUICreateScripts(Rect rect) {
       
            EditorGUI.BeginDisabledGroup(true);

            // 搜索框
            if (_searchField == null)
            {
                _searchField = new SearchField();
            }
            _searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 5, SearchHeight);
            _stateTree.searchString = _searchField.OnGUI(_searchRect, _stateTree.searchString);

            EditorGUI.EndDisabledGroup();

            // 标签
            _labelRect.Set(rect.x, rect.y + SearchHeight, rect.width, LabelHeight);

            if (GUI.Button(_labelRect, "New Script", "AC BoldHeader")) 
            {
                _showCreateScriptGUI = false;
            }
            _labelRect.y -= 3;
            GUI.Label(_labelRect, EditorGUIUtility.IconContent("ArrowNavigationLeft"));

            // 参数列表 

            _stateRect.Set(rect.x, rect.y + SearchHeight + LabelHeight - 5, rect.width, rect.height - SearchHeight - LabelHeight - 20);

            GUILayout.BeginArea(_stateRect);
            GUILayout.Space(10);
            GUILayout.Label("Name");
            EditorGUI.BeginChangeCheck();

            _scriptName2 = EditorGUILayout.DelayedTextField(_scriptName);
                
            if (EditorGUI.EndChangeCheck()) 
            { 
                _scriptName = _scriptName2;
                EditorApplication.delayCall += CreateAndAddScript;
            } 
            GUILayout.EndArea();

            _stateRect.Set(rect.x,rect.height - 23, rect.width, 23);
            if (GUI.Button(_stateRect, "Create And Add", "AppToolbarButtonMid"))
            {
                OnButtonClick();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return ) 
            {
                EditorApplication.delayCall += CreateAndAddScript; 
            }

        }

        private void Update() { 
            
            if(!_showCreateScriptGUI && editorWindow != null)
                editorWindow.Repaint(); 
             
            if (EditorWindow.focusedWindow != editorWindow)
            {
                EditorApplication.update -= Update;
            }
        }
        
        public override void OnOpen()
        {
            base.OnOpen(); 
            EditorApplication.update += Update;
        }

        public override void OnClose()
        {
            base.OnClose();
            EditorApplication.update -= Update;
        }

        public void Close() 
        {
            EditorApplication.update -= Update;
            if (editorWindow == null)  
                return; 

            editorWindow.Close(); 
        }
        
        private void CreateAndAddScript() {

            if (EditorWindow.focusedWindow != editorWindow)
                return;

            if (string.IsNullOrEmpty(_scriptName2)) 
                return;

            FSMStateGraphView.SavePrefsSelection(this._nodeData.name);
            
            string dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_controller));
            string path = $"{dir}/{_scriptName2}.cs";
            bool isSuccess = FSMStateCreator.CreateFSMState(path, false);

            if (!isSuccess)
            {
                GUIContent content = new GUIContent($"名称:{_scriptName}不可用,请修改后重试!");
                this.editorWindow.ShowNotification(content);
                return;
            }

            EditorApplication.delayCall += () => 
            {             
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                EditorGUIUtility.PingObject(script);
                this._nodeData.AddStateScript(script);
                _controller.Save();
                AssetDatabase.SaveAssets();
            };
        }
        
        private void OnButtonClick() 
        {
            GUI.FocusControl(null);
        }
    }
}
