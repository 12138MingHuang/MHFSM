using System.Collections.Generic;
using UnityEngine;

namespace MHFSM
{
    /// <summary>
    /// 状态之间的过渡
    /// </summary>
    public class FSMTransition
    {
        private FSMTransitionData _data;
        private RuntimeFSMControllerInstance _controller;
        private FSMStateNode _toState;

        public List<FSMConditionGroup> conditionGroups;
        
        /// <summary>
        /// 过渡数据
        /// </summary>
        public FSMTransitionData Data => _data;
        
        /// <summary>
        /// 过渡目标状态
        /// </summary>
        public FSMStateNode ToState => _toState;

        internal bool IsMeet
        {
            get
            {
                if (!ConditionGroupMeet())
                    return false;

                if (_toState == null)
                {
                    Debug.LogError("查询目标状态失败!");
                    return false;
                }

                FSMStateNodeData from = _controller.RuntimrFSMController.GetStateNodeData(_data.fromStateName);
                string parent = from.Parent;
                FSMStateNode currentState = _controller.GetCurrentState(parent);

                if (currentState == null) return false;

                if (!_data.fromStateName.Equals(currentState.data.name))
                {
                    if (!from.IsAnyState) return false;

                    // 判断当前的状态 跟 目标状态是不是一个 
                    if (currentState.data.name.Equals(_data.toStateName)) return false;
                }

                return true;
            }
        }

        internal FSMTransition(RuntimeFSMControllerInstance controller, FSMTransitionData data)
        {
            _data = data;
            _controller = controller;
            conditionGroups = new List<FSMConditionGroup>();
            FSMConditionGroup group = new FSMConditionGroup(controller, _data.conditions, data);
            group.onConditionMeet += CheckCondition;
            conditionGroups.Add(group);

            foreach (GroupCondition condition in data.groupConditions)
            {
                FSMConditionGroup conditionGroup = new FSMConditionGroup(controller, condition.conditions, data);
                conditionGroup.onConditionMeet += CheckCondition;
                conditionGroups.Add(conditionGroup);
            }
            
            if(controller.states.ContainsKey(data.toStateName))
                _toState = controller.states[data.toStateName];
        }
        
        
        private void CheckCondition()
        {
            CheckConditionAndSwitch();
        }
        
        /// <summary>
        /// 检查条件是否满足，并切换状态
        /// </summary>
        /// <returns> 是否满足条件 </returns>
        internal bool CheckConditionAndSwitch()
        {
            bool isMeet = IsMeet;
            if (isMeet)
            {
                ResetTrigger();
                // 切换状态
                _controller.SwitchState(_toState, this);
            }
            
            return isMeet;
        }
        private void ResetTrigger()
        {
            foreach (FSMConditionGroup conditionGroup in conditionGroups)
            {
                conditionGroup.ResetTrigger();
            }
        }

        private bool ConditionGroupMeet()
        {
            foreach (FSMConditionGroup conditionGroup in conditionGroups)
            {
                if (conditionGroup.IsMeet)
                    return true;
            }
            
            return false;
        }
    }
}
