using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace MHFSM
{
    public class RuntimeFSMControllerCreator : EndNameEditAction
    {

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            RuntimeFSMController fsmController = ScriptableObject.CreateInstance<RuntimeFSMController>();
            AssetDatabase.CreateAsset(fsmController, pathName);
            Selection.activeObject = fsmController;

            Rect rect = new Rect(0, 100, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);
            FSMStateNodeFactory.CreateStateNode(fsmController, FSMConst.AnyState, rect, false, false, null, true, FSMConst.AnyState);
            rect = new Rect(0, 300, FSMConst.StateNodeWidth, FSMConst.StateNodeHeight);
            FSMStateNodeFactory.CreateStateNode(fsmController, FSMConst.EntryState, rect, false, false, null, true, FSMConst.EntryState);
            
        }
    }
}
