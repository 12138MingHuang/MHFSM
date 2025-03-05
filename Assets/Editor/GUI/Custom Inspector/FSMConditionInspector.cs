using UnityEngine;

namespace MHFSM
{
    public class FSMConditionInspector
    {
        public virtual void OnGUI(Rect rect, FSMConditionData condition, RuntimeFSMController controller) { }
    }
}