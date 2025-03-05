using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class FSMBoolConditionInspector : FSMConditionInspector
    {
        public override void OnGUI(Rect rect, FSMConditionData condition, RuntimeFSMController controller)
        {
            string text = condition.targetValue == 1 ? "True" : "False";

            if (EditorGUI.DropdownButton(rect, new GUIContent(text), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("True"), condition.targetValue == 1, () =>
                {
                    condition.targetValue = 1;
                    controller.Save();
                });

                menu.AddItem(new GUIContent("False"), condition.targetValue == 0, () =>
                {
                    condition.targetValue = 0;
                    controller.Save();
                });
                
                menu.ShowAsContext();
            }
        }
    }
}
