using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MHFSM
{
    public class FSMStateGraphView : GraphView
    {
        public Dictionary<string, FSMNodeView> StateNodes = new Dictionary<string, FSMNodeView>();

        internal FSMTransitionBackground transition = null;

        internal FSMRectangleSelector selector;

        public bool isMakeTransition = false;

        public FSMNodeView MakeTransitionFrom = null;

        public FSMNodeView HoverNode = null;

        private RuntimeFSMController controller = null;

        public FSMStateGraphView()
        {
            FSMGridBackground background = new FSMGridBackground();
            Insert(0, background);

            transition = new FSMTransitionBackground();
            Insert(1, transition);

            selector = new FSMRectangleSelector();
            Insert(3, selector);

            RegisterCallback<KeyDownEvent>(KeyDownControl);
            RegisterCallback<MouseDownEvent>(MouseDownControl);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new FSMSelectionDragger());
            ContentZoomer zoomer = new ContentZoomer();
            zoomer.minScale = 0.1f;
            zoomer.maxScale = 3;

            this.AddManipulator(zoomer);

            RefreshView();
        }

        private void RefreshView()
        {
            if (Context.Instance.RuntimeFSMController != null)
            {
                // 默认位置和缩放
                viewTransform.position = Context.Instance.RuntimeFSMController.viewPosition;
                viewTransform.scale = Context.Instance.RuntimeFSMController.viewScale;
            }
            controller = Context.Instance.RuntimeFSMController;
            RefreshNodes();
        }

        private void SaveScaleAndPosition()
        {

            if (viewTransform == null || Context.Instance.RuntimeFSMController == null)
                return;


            if (Context.Instance.RuntimeFSMController.viewScale != viewTransform.scale)
            {
                Context.Instance.RuntimeFSMController.viewScale = viewTransform.scale;
                Context.Instance.RuntimeFSMController.Save();
            }


            if (Context.Instance.RuntimeFSMController.viewPosition != viewTransform.position)
            {
                Context.Instance.RuntimeFSMController.viewPosition = viewTransform.position;
                Context.Instance.RuntimeFSMController.Save();
            }

        }

        public void Update()
        {
            if (controller != Context.Instance.RuntimeFSMController)
            {
                RefreshView();
                controller = Context.Instance.RuntimeFSMController;
            }

            SaveScaleAndPosition();
        }

        public void RefreshNodes()
        {

            foreach (var item in StateNodes.Values)
            {
                RemoveElement(item);
            }

            StateNodes.Clear();

            List<FSMStateNodeData> datas = Context.Instance.GetCurrentShowStateNodeData();

            if (datas == null || datas.Count == 0)
                return;

            //selectNode = null;

            foreach (var item in datas)
            {
                FSMNodeView node = new FSMNodeView(item, this);
                node.title = item.DisplayName;
                node.SetPosition(item.rect);
                this.AddElement(node);
                StateNodes.Add(item.name, node);

                if (GetPrefsSelection(item.name))
                {
                    AddToSelection(node);
                }
            }


        }

        public void RenameState(string oldName, string newName)
        {
            //Debug.LogFormat("重命名:old:{0} new:{1} cn:{2} co:{3}",oldName,newName, StateNodes.ContainsKey(newName), StateNodes.ContainsKey(oldName));
            if (StateNodes.ContainsKey(newName))
                return;
            if (!StateNodes.ContainsKey(oldName))
                return;
            StateNodes.Add(newName, StateNodes[oldName]);
            StateNodes.Remove(oldName);
        }


        public FSMNodeView GetNode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (StateNodes.ContainsKey(name))
                return StateNodes[name];

            return null;
        }


        private void KeyDownControl(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete)
            {
                RuntimeFSMController controller = Context.Instance.RuntimeFSMController;

                if (controller == null) return;

                // 删除选中的过渡
                if (transition.Selection != null && controller != null)
                    FSMTransitionFactory.DeleteTransition(controller, transition.Selection);

                RefreshNodes();
            }
        }

        private void MouseDownControl(MouseDownEvent evt)
        {
            //BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
            if (evt.button == 0 && evt.clickCount == 1)
            {
                // 左键单击
                transition.OnTransitionSelection(transition.CloseTransition);
                StopMakeTransition(null);
            }
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {

            FSMNodeView node = evt.target as FSMNodeView;

            if (node != null)
                transition.OnTransitionSelection(null);


            int count = evt.menu.MenuItems().Count;
            for (int i = 0; i < count; i++)
            {
                evt.menu.RemoveItemAt(0);
            }

            RuntimeFSMController controller = Context.Instance.RuntimeFSMController;

            bool disable = controller == null || EditorApplication.isPlaying;

            DropdownMenuAction.Status status = disable ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;

            if (node == null)
            {

                //Debug.LogFormat("鼠标位置:{0}",Event.current.mousePosition);

                evt.menu.AppendAction("Create State", CreateState, (d) => status, Event.current.mousePosition);
                evt.menu.AppendAction("Create Sub-State Machine", CreateSubStateMachine, (d) => status, Event.current.mousePosition);
            }
            else
            {
                node.ShowMenu(evt);
            }

            //base.BuildContextualMenu

        }


        public void CreateState(DropdownMenuAction action)
        {
            Vector2 mousePosition = (Vector2)(action.userData);
            mousePosition.Set(mousePosition.x - parent.layout.x, mousePosition.y - layout.y - 20);
            Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint(mousePosition);
            CreateState(false, position);
        }

        public void CreateSubStateMachine(DropdownMenuAction action)
        {
            Vector2 mousePosition = (Vector2)(action.userData);
            mousePosition.Set(mousePosition.x - parent.layout.x, mousePosition.y - layout.y - 20);
            Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint(mousePosition);
            CreateState(true, position);
        }


        private void CreateState(bool isSubStateMachine, Vector3 mousePosition)
        {

            Rect rect = new Rect(0, 0, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);

            rect.center = mousePosition;

            List<FSMStateNodeData> states = Context.Instance.GetCurrentShowStateNodeData();

            bool isDefaultState = states.Count == 2; // 这里是为了兼容旧的配置文件

            if (states != null)
            {
                bool isAllBuildIn = true;
                foreach (var item in states)
                {
                    if (!item.isBuildInState)
                    {
                        isAllBuildIn = false;
                        break;
                    }
                }
                if (isAllBuildIn) isDefaultState = true;
            }


            RuntimeFSMController controller = Context.Instance.RuntimeFSMController;

            List<string> parents = new List<string>();
            parents.AddRange(Context.Instance.Layers);

            FSMStateNodeData state = FSMStateNodeFactory.CreateStateNode(controller, rect, isDefaultState, isSubStateMachine, parents);

            if (isSubStateMachine)
            {
                // 给当前子状态机创建 Entry 状态
                Rect r = new Rect(0, 300, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);
                CreateSubState(controller, FSMConst.EntryState, state, parents, r, FSMConst.EntryState);
                // 给当前子状态机创建 Any 状态
                r = new Rect(0, 100, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);
                CreateSubState(controller, FSMConst.AnyState, state, parents, r, FSMConst.AnyState);
                // 给当前子状态机创建 Entry 状态
                r = new Rect(600, 300, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);
                CreateSubState(controller, FSMConst.Up, state, parents, r, FSMConst.Up);
            }


            RefreshNodes();
        }

        private void CreateSubState(RuntimeFSMController controller, string name, FSMStateNodeData state, List<string> parents, Rect rect, string buildInName)
        {
            // 创建三个内置状态
            List<string> parents_temp = new List<string>();
            parents_temp.AddRange(parents);
            parents_temp.Add(state.name);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < parents_temp.Count; i++)
            {
                stringBuilder.Append(parents_temp[i]);
                stringBuilder.Append("/");
            }
            stringBuilder.Append(name);
            FSMStateNodeFactory.CreateStateNode(controller, stringBuilder.ToString(), rect, false, false, parents_temp, true, buildInName);
        }


        public void StartMakeTransition(FSMNodeView node)
        {
            isMakeTransition = true;
            MakeTransitionFrom = node;
        }

        public void StopMakeTransition(FSMNodeView node)
        {
            if (node == MakeTransitionFrom)
                return;

            isMakeTransition = false;
            HoverNode = null;

            if (node == null)
                return;

            RuntimeFSMController controller = Context.Instance.RuntimeFSMController;
            if (controller == null)
                return;
            FSMTransitionFactory.CreateTransition(controller, MakeTransitionFrom.Data.name, node.Data.name);
        }

        public void OnGUI()
        {

            // 取消拖拽
            if (Event.current.type == EventType.MouseLeaveWindow && Event.current.button == 0)
            {
                selector.CancelDrag();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                selector.CancelDrag();
            }

        }


        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);
            selector.ExecuteAction(evt);
        }

        internal static bool GetPrefsSelection(string name)
        {
            string key = PrefsSelectionKey(name);
            if (string.IsNullOrEmpty(key))
                return false;
            bool v = EditorPrefs.GetBool(key, false);
            if (v)
                EditorPrefs.DeleteKey(key);
            return v;
        }

        internal static void SavePrefsSelection(string name)
        {
            string key = PrefsSelectionKey(name);
            if (string.IsNullOrEmpty(key))
                return;
            EditorPrefs.SetBool(key, true);
        }

        internal static string PrefsSelectionKey(string name)
        {
            RuntimeFSMController controller = Context.Instance.RuntimeFSMController;
            if (controller == null)
                return string.Empty;

            return $"MHFSM:{controller.name}:{name}";
        }
    }
}
