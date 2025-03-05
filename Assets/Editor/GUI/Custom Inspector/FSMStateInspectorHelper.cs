using UnityEditor;

namespace MHFSM
{
    public class FSMStateInspectorHelper : ScriptableObjectSingleton<FSMStateInspectorHelper>
    {
        public FSMStateNodeData node;
        public RuntimeFSMController controller;
        public FSMStateGraphView graphView;

        public void Inspect(RuntimeFSMController controller, FSMStateNodeData node, FSMStateGraphView graphView)
        {
            if (node == null)
            {
                Selection.activeObject = null;
                return;
            }
            
            this.controller = controller;
            this.node = node;
            this.graphView = graphView;
            Selection.activeObject = this;
        }
    }
}
