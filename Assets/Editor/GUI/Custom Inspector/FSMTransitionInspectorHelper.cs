using UnityEditor;

namespace MHFSM
{
    public class FSMTransitionInspectorHelper : ScriptableObjectSingleton<FSMTransitionInspectorHelper>
    {
        public FSMTransitionData transitionData;
        public RuntimeFSMController controller;

        public void Inspect(RuntimeFSMController controller, FSMTransitionData transitionData)
        {
            if (transitionData == null)
            {
                Selection.activeObject = null;
                return;
            }
            
            this.transitionData = transitionData;
            this.controller = controller;
            
            Selection.activeObject = this;
        }
    }
}
