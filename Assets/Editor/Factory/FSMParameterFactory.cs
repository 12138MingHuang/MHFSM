using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class FSMParameterFactory
    {
        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="controller"> fsm控制器
        /// <param name="type"> 参数类型</param>
        public static void CreateParameter(RuntimeFSMController controller, ParameterType type)
        {
            FSMParameterData parameterData = new FSMParameterData();
            parameterData.name = GetDefaultName(controller, type);
            parameterData.parameterType = type;
            parameterData.value = 0;
            
            controller.AddParameters(parameterData);
        }
        private static string GetDefaultName(RuntimeFSMController controller, ParameterType type)
        {
            string name = $"New {type.ToString()}";
            string tempName = name;

            int i = 1;
            while (controller.GetParameterData(tempName) != null)
            {
                tempName = $"{name}{i}";
                i++;
            }
            
            return tempName;
        }

        /// <summary>
        /// 删除参数
        /// </summary>
        /// <param name="controller"> fsm控制器</param>
        /// <param name="index"> 参数索引</param>
        public static void RemoveParameter(RuntimeFSMController controller, int index)
        {
            if (Application.isPlaying) return;
            if (controller == null) return;

            FSMParameterData parameterData = controller.parameters[index];
            
            List<FSMTransitionData> transitions = new List<FSMTransitionData>();
            
            // 查询引用了这个参数的过渡
            foreach (FSMTransitionData transition in controller.transitions)
            {
                foreach (FSMConditionData condition in transition.conditions)
                {
                    if (condition.parameterName != null && condition.parameterName.Equals(parameterData.name))
                    {
                        transitions.Add(transition);
                        break;
                    }
                }
            }

            if (transitions.Count == 0)
                controller.RemoveParameters(parameterData);
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("确定删除参数:").Append(parameterData.name).Append("吗?").Append("\n");
                sb.Append($"有以下过渡引用此参数").Append("\n");

                foreach (FSMTransitionData transition in transitions)
                {
                    sb.Append(transition.fromStateName).Append(" -> ").Append(transition.toStateName);
                }

                if (EditorUtility.DisplayDialog("删除参数", sb.ToString(), "确定", "取消"))
                {
                    controller.RemoveParameters(parameterData);

                    foreach (FSMTransitionData transition in transitions)
                    {
                        for (int i = transition.conditions.Count - 1; i >= 0; i--)
                        {
                            FSMConditionData condition = transition.conditions[i];
                            if (condition.parameterName != null && condition.parameterName.Equals(parameterData.name))
                                transition.conditions.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public static void RenameParameter(RuntimeFSMController controller, FSMParameterData parameterData, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                // TODO: 弹出提示
                Debug.LogError("参数名称不能为空!");
                return;
            }

            if (controller.GetParameterData(newName) != null)
            {
                
                // TODO: 弹出提示
                Debug.LogError("参数名称已存在!");
                return;
            }
            
            // 查找所有引用该参数的过渡 修改名称
            foreach (FSMTransitionData transition in controller.transitions)
            {
                foreach (FSMConditionData condition in transition.conditions)
                {
                    if(condition.parameterName != null && condition.parameterName.Equals(parameterData.name))
                        condition.parameterName = newName;
                }
                
                // 遍历其他组的条件
                foreach (GroupCondition groupCondition in transition.groupConditions)
                {
                    foreach (FSMConditionData condition in groupCondition.conditions)
                    {
                        if (condition.parameterName != null && condition.parameterName.Equals(parameterData.name))
                            condition.parameterName = newName;
                    }
                }
            }
            
            // 修改参数名称
            parameterData.name = newName;
            controller.ClearCache();
            controller.Save();
        }
    }
}
