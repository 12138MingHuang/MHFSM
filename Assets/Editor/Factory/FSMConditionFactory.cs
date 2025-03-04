using System;
using UnityEditor;

namespace MHFSM
{
    public class FSMConditionFactory
    {
        public static FSMConditionData CreateCondition(RuntimeFSMController controller)
        {
            FSMConditionData condition = new FSMConditionData();
            
            string parameterName = string.Empty;

            FSMParameterData parameter = null;

            if (controller.parameters.Count > 0)
            {
                parameter = controller.parameters[0];
                parameterName = parameter.name;
            }

            if (parameter != null)
            {
                switch (parameter.parameterType)
                {

                    case ParameterType.Float:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Int:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        condition.compareType = CompareType.Equal;
                        break;
                }
            }
            else
            {
                condition.compareType = CompareType.Greater;
            }
            
            condition.targetValue = 0;
            condition.parameterName = parameterName;
            
            return condition;
        }
        /// <summary>
        /// 创建条件并添加到转换中。
        /// </summary>
        /// <param name="controller"> FSM控制器</param>
        /// <param name="transition"> 转换</param>
        public static void CreateCondition(RuntimeFSMController controller, FSMTransitionData transition)
        {
            FSMConditionData condition = CreateCondition(controller);
            transition.conditions.Add(condition);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 删除转换中的条件。
        /// </summary>
        /// <param name="controller"> FSM控制器</param>
        /// <param name="transition"> 转换</param>
        /// <param name="index"> 条件索引</param>
        public static void DeleteCondition(RuntimeFSMController controller, FSMTransitionData transition, int index)
        {
            if (index < 0 || index >= transition.conditions.Count)
                return;
            
            transition.conditions.RemoveAt(index);
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
    }
}
