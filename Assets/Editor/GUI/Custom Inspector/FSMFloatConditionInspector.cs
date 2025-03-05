using System;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class FSMFloatConditionInspector : FSMConditionInspector
    {
        private Rect _leftRect;
        private Rect _rightRect;

        public override void OnGUI(Rect rect, FSMConditionData condition, RuntimeFSMController controller)
        {
            _leftRect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            _rightRect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (EditorGUI.DropdownButton(_leftRect, new GUIContent(condition.compareType.ToString()), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < Enum.GetValues(typeof(CompareType)).Length; i++)
                {
                    CompareType compareType = (CompareType)Enum.GetValues(typeof(CompareType)).GetValue(i);

                    if (compareType == CompareType.Equal || compareType == CompareType.NotEqual) continue;
                    
                    menu.AddItem(new GUIContent(compareType.ToString()), condition.compareType == compareType, () =>
                    {
                        condition.compareType = compareType;
                        controller.Save();
                    });
                }
                
                menu.ShowAsContext();
            }
            
            condition.targetValue = EditorGUI.FloatField(_rightRect, condition.targetValue);
            EditorUtility.SetDirty(controller);
        }
    }
}
