using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class FSMTransitionFactory
    {
        public static void CreateTransition(RuntimeFSMController controller, string fromStateName, string toStateName)
        {
            FSMStateNodeData toNode = controller.GetStateNodeData(toStateName);

            if (toNode.IsAnyState || toNode.IsEntryState || toNode.IsUpstate)
            {
                string message = $"状态:{toNode.DisplayName}不能添加过渡!";
                Debug.LogError(message);
                // TODO: 窗口抛出异常
                return;
            }
            
            if(fromStateName.Equals(toStateName))
                return;

            foreach (FSMTransitionData transitionData in controller.transitions)
            {
                if (transitionData.fromStateName.Equals(fromStateName) && transitionData.toStateName.Equals(toStateName))
                {
                    string message = $"过渡 {fromStateName} -> {toStateName} 已存在,请勿重复添加!";
                    Debug.LogError(message);
                    // TODO: 窗口抛出异常
                    return;
                }
            }
            
            FSMTransitionData transition = new FSMTransitionData();
            transition.fromStateName = fromStateName;
            transition.toStateName = toStateName;
            controller.AddTransition(transition);
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 删除过渡
        /// </summary>
        /// <param name="controller"> FSM控制器</param>
        /// <param name="transition"> 过渡</param>
        public static void DeleteTransition(RuntimeFSMController controller, FSMTransitionData transition)
        {
            controller.RemoveTransition(transition);
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
    }
}